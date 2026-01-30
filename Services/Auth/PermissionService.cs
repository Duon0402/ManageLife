using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class PermissionService : ServiceBase, IPermissionService
    {
        private readonly ICacheService _cache;
        private readonly PermissionRepository _repoPermission;
        private readonly UserPermissionRepository _repoUserPermission;
        private readonly UserRoleRepository _repoUserRole;
        private readonly RoleRepository _repoRole;
        private readonly UserRepository _repoUser;
        private readonly RolePermissionRepository _repoRolePermission;

        public PermissionService(AppDbContext context, ICacheService cache) : base(context)
        {
            _cache = cache;
            _repoPermission = new PermissionRepository(context);
            _repoUserPermission = new UserPermissionRepository(context);
            _repoRolePermission = new RolePermissionRepository(context);
            _repoUserRole = new UserRoleRepository(context);
            _repoRole = new RoleRepository(context);
            _repoUser = new UserRepository(context);
        }

        public async Task<Result<List<PermissionModel>>> GetListPermissionsAsync()
        {
            string msg;
            try
            {
                var models = new List<PermissionModel>();

                var entities = await _repoPermission.GetAllAsync();

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<PermissionModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserIdAsync(GetAssignedPermissionsByUserIdRequest request)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    string msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }

                var cacheItem = CacheItems.Permissions(request.UserId);

                var cachedPermissions = await _cache.TryGetValueAsync<List<PermissionModel>>(cacheItem);
                if (cachedPermissions.IsNotEmpty())
                {
                    return Result.Ok(cachedPermissions);
                }

                var permissionIds = await _repoUserPermission.Query(true)
                    .Where(up => up.UserId == request.UserId)
                    .Select(up => up.PermissionId)
                    .Concat(
                        _repoUserRole.Query(true)
                            .Where(ur => ur.UserId == request.UserId
                                      && ur.Status == UserPermissionStatus.Grant)
                            .Join(
                                _repoRolePermission.Query(true),
                                ur => ur.RoleId,
                                rp => rp.RoleId,
                                (ur, rp) => rp.PermissionId
                            )
                    )
                    .Distinct()
                    .ToListAsync();

                var permissions = await _repoPermission.Query(true)
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToListAsync();

                var models = permissions.IsNotEmpty()
                    ? permissions.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                await _cache.SetAsync(models, cacheItem);
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                return Result.Exception<List<PermissionModel>>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result> SyncPermissionsAsync(List<string> permissionCodes)
        {
            using var uow = new UnitOfWork(_context);
            bool clearPermissionCache = false;

            try
            {
                var dbPermissions = await _repoPermission.GetAllAsync();
                var dbPermissionCodes = dbPermissions.Select(p => p.Code).ToList();

                var toInsertCodes = permissionCodes.Except(dbPermissionCodes).ToList();
                var toDelete = dbPermissions.Where(p => p.Code.NotIn(permissionCodes)).ToList();

                var insertPermissions = toInsertCodes.Select(code => new PermissionEntity
                {
                    Id = IdHeper.NewId(),
                    Code = code,
                    Name = code,
                    CreatedUser = SystemUsers.System
                }).ToList();

                if (insertPermissions.IsNotEmpty())
                {
                    if (!await _repoPermission.BulkInsertAsync(insertPermissions, uow))
                        return Result.DATA_NOT_CREATE;

                    clearPermissionCache = true;
                }

                if (toDelete.IsNotEmpty())
                {
                    if (!await _repoPermission.BulkDeleteAsync(toDelete, uow))
                        return Result.DATA_NOT_DELETE;

                    clearPermissionCache = true;
                }

                var adminRole = await _repoRole.Query()
                    .FirstOrDefaultAsync(x => x.Name == RoleConst.Admin);

                var userAdminIds = new List<string>();
                if (adminRole != null)
                {
                    if (toDelete.IsNotEmpty())
                    {
                        var toDeleteIds = toDelete.Select(p => p.Id).ToList();
                        var adminMappingsToDelete = await _repoRolePermission.Query()
                            .Where(rp => rp.RoleId == adminRole.Id && toDeleteIds.Contains(rp.PermissionId))
                            .ToListAsync();

                        if (adminMappingsToDelete.IsNotEmpty())
                        {
                            if (!await _repoRolePermission.BulkDeleteAsync(adminMappingsToDelete, uow))
                                return Result.DATA_NOT_DELETE;

                            clearPermissionCache = true;
                        }
                    }

                    if (insertPermissions.IsNotEmpty())
                    {
                        var rolePermissions = insertPermissions.Select(p => new RolePermissionEntity
                        {
                            RoleId = adminRole.Id,
                            PermissionId = p.Id
                        }).ToList();

                        if (!await _repoRolePermission.BulkInsertAsync(rolePermissions, uow))
                            return Result.DATA_NOT_CREATE;

                        clearPermissionCache = true;
                    }

                    if (clearPermissionCache)
                    {
                        userAdminIds = await _repoUserRole.Query(true).Where(x => x.RoleId == adminRole.Id).Select(x => x.UserId).ToListAsync();
                    }
                }

                await uow.CommitAsync();

                if (clearPermissionCache && userAdminIds.IsNotEmpty())
                {
                    var cacheItems = userAdminIds.SelectDistinctToList(id => CacheItems.Permissions(id));

                    await _cache.RemoveAsync(cacheItems);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserIdAsync(GetUnassignedPermissionsByUserIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }

                var rsAssignedPermissions = await GetAssignedPermissionsByUserIdAsync(new GetAssignedPermissionsByUserIdRequest
                {
                    UserId = request.UserId
                });

                if (rsAssignedPermissions.IsError())
                {
                    msg = rsAssignedPermissions.Message;
                    return Result.Error<List<PermissionModel>>(rsAssignedPermissions.Code, msg);
                }

                var rsAllPermissions = await GetListPermissionsAsync();
                if (rsAssignedPermissions.IsError())
                {
                    msg = rsAllPermissions.Message;
                    return Result.Error<List<PermissionModel>>(rsAllPermissions.Code, msg);
                }

                var assignedPermissions = rsAssignedPermissions.Data ?? new List<PermissionModel>();
                var allPermissions = rsAllPermissions.Data ?? new List<PermissionModel>();
                var assignedCodes = new HashSet<string>(assignedPermissions.Select(p => p.Code));
                var unassignedPermissions = allPermissions.Where(p => p.Code.NotIn(assignedCodes)).ToList();

                return Result.Ok(unassignedPermissions);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result> AssignPermissionsAsync(AssignPermissionsRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                if (request.PermissionIds.IsEmpty())
                {
                    msg = "Dữ liệu đầu vào không hợp lệ";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var user = await _repoUser.GetAsync(x => x.Id == request.UserId);
                if (user == null)
                {
                    msg = "Không tìm thấy người dùng cần gán quyền";
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                //var cacheKeyItem = CacheKey.Permissions(request.UserId, TimeSpan.FromMinutes(30));

                //await _cache.RemoveAsync(cacheKeyItem.Key);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception(msg, ex);
            }
        }

        public Task<Result> UnassignPermissionsAsync(UnassignPermissionsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
