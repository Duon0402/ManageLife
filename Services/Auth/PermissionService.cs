using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ICacheService _cache;
        private readonly IPermissionRepository _repoPermission;
        private readonly IUserPermissionRepository _repoUserPermission;
        private readonly IUserRoleRepository _repoUserRole;
        private readonly IRoleRepository _repoRole;
        private readonly IUserRepository _repoUser;
        private readonly IRolePermissionRepository _repoRolePermission;
        private readonly AppDbContext _context;

        public PermissionService(
            IPermissionRepository repoPermission,
            IUserPermissionRepository repoUserPermission,
            IRolePermissionRepository repoRolePermission,
            IUserRoleRepository repoUserRole,
            IRoleRepository repoRole,
            IUserRepository repoUser,
            ICacheService cache,
            AppDbContext context)
        {
            _cache = cache;
            _repoPermission = repoPermission;
            _repoUserPermission = repoUserPermission;
            _repoRolePermission = repoRolePermission;
            _repoUserRole = repoUserRole;
            _repoRole = repoRole;
            _repoUser = repoUser;
            _context = context;
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

                var cacheItem = CacheSettings.Permissions(request.UserId);

                var cachedPermissions = await _cache.TryGetValueAsync<List<PermissionModel>>(cacheItem);
                if (cachedPermissions.IsNotEmpty())
                {
                    return Result.Ok(cachedPermissions);
                }

                var permissions = await _repoPermission.Query(true)
                    .Where(p =>
                        _repoUserPermission.Query(true)
                            .Any(up =>
                                up.UserId == request.UserId &&
                                up.PermissionId == p.Id &&
                                up.Status == UserPermissionStatus.Grant
                            )

                        ||

                        (
                            !_repoUserPermission.Query(true)
                                .Any(up =>
                                    up.UserId == request.UserId &&
                                    up.PermissionId == p.Id &&
                                    up.Status == UserPermissionStatus.Deny
                                )
                            &&
                            _repoUserRole.Query(true)
                                .Where(ur => ur.UserId == request.UserId)
                                .Join(
                                    _repoRolePermission.Query(true),
                                    ur => ur.RoleId,
                                    rp => rp.RoleId,
                                    (ur, rp) => rp.PermissionId
                                )
                                .Any(pid => pid == p.Id)
                        )
                    )
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
                    var cacheItems = userAdminIds.SelectDistinctToList(id => CacheSettings.Permissions(id));

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

                var permissions = await _repoPermission.Query(true)
                    .Where(p =>
                        !(
                            _repoUserPermission.Query(true)
                                .Any(up =>
                                    up.UserId == request.UserId &&
                                    up.PermissionId == p.Id &&
                                    up.Status == UserPermissionStatus.Grant
                                )

                            ||
                            (
                                !_repoUserPermission.Query(true)
                                    .Any(up =>
                                        up.UserId == request.UserId &&
                                        up.PermissionId == p.Id &&
                                        up.Status == UserPermissionStatus.Deny
                                    )
                                &&
                                _repoUserRole.Query(true)
                                    .Where(ur => ur.UserId == request.UserId)
                                    .Join(
                                        _repoRolePermission.Query(true),
                                        ur => ur.RoleId,
                                        rp => rp.RoleId,
                                        (ur, rp) => rp.PermissionId
                                    )
                                    .Any(pid => pid == p.Id)
                            )
                        )
                    )
                    .ToListAsync();

                var models = permissions.IsNotEmpty()
                    ? permissions.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result> AssignPermissionsAsync(AssignPermissionsRequest request)
        {
            //TODO: Hoàn thành code phần gán quyền và gỡ quyền
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

                var userExists = await _repoUser.FirstOrDefaultAsync(x => x.Id == request.UserId && x.IsDeleted == false);
                if (userExists == null)
                {
                    msg = "Không tìm thấy người dùng cần gán quyền";
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var inputPermissionSet = request.PermissionIds
                    .Distinct()
                    .ToHashSet();

                var rolePermissionIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.UserId == request.UserId)
                    .Join(
                        _repoRolePermission.Query(true),
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.PermissionId
                    )
                    .Distinct()
                    .ToListAsync();

                var rolePermissionSet = rolePermissionIds.ToHashSet();

                var userPermissions = await _repoUserPermission.Query()
                    .Where(up => up.UserId == request.UserId)
                    .ToListAsync();

                var userPermissionDict = userPermissions
                    .ToDictionary(x => x.PermissionId);



                //var cacheItem = CacheSettings.Permissions(request.UserId);
                //await _cache.RemoveAsync(cacheItem);

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
