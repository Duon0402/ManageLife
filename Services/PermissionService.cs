using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class PermissionService : ServiceBase<PermissionService>, IPermissionService
    {
        private readonly ICacheService _cache;
        private readonly IPermissionRepository _repoPermission;
        private readonly IUserPermissionRepository _repoUserPermission;
        private readonly IUserRoleRepository _repoUserRole;
        private readonly IRoleRepository _repoRole;
        private readonly IUserRepository _repoUser;
        private readonly IPermissionGuard _permissionGuard;
        private readonly IUnitOfWork _uow;
        private readonly IRolePermissionRepository _repoRolePermission;

        public PermissionService(
            IPermissionRepository repoPermission,
            IUserPermissionRepository repoUserPermission,
            IRolePermissionRepository repoRolePermission,
            IUserRoleRepository repoUserRole,
            IRoleRepository repoRole,
            IUserRepository repoUser,
            ICacheService cache,
            IAppLogger<PermissionService> logger,
            IPermissionGuard permissionGuard,
            IUnitOfWork uow,
            IUserContext userContext) : base(logger, userContext)
        {
            _cache = cache;
            _repoPermission = repoPermission;
            _repoUserPermission = repoUserPermission;
            _repoRolePermission = repoRolePermission;
            _repoUserRole = repoUserRole;
            _repoRole = repoRole;
            _repoUser = repoUser;
            _permissionGuard = permissionGuard;
            _uow = uow;
        }

        public async Task<Result<List<PermissionModel>>> GetListPermissionsAsync(CancellationToken ct = default)
        {
            try
            {
                var models = new List<PermissionModel>();

                var entities = await _repoPermission.GetAllAsync(ct);

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<PermissionModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserIdAsync(GetAssignedPermissionsByUserIdRequest request, CancellationToken ct = default)
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
                    .ToListAsync(ct);

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

        public async Task<Result> SyncPermissionsAsync(List<string> permissionCodes, CancellationToken ct = default)
        {
            bool clearPermissionCache = false;
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var dbPermissions = await _repoPermission.GetAllAsync(ct);
                var dbPermissionCodes = dbPermissions.Select(p => p.Code).ToList();

                var toInsertCodes = permissionCodes.Except(dbPermissionCodes).ToList();
                var toDelete = dbPermissions.Where(p => p.Code.NotIn(permissionCodes)).ToList();

                var insertPermissions = toInsertCodes.Select(code => new PermissionEntity
                {
                    Id = IdHelper.NewId(),
                    Code = code,
                    Name = code,
                    CreatedUser = SystemUsers.System
                }).ToList();

                if (insertPermissions.IsNotEmpty())
                {
                    if (!await _repoPermission.BulkInsertAsync(insertPermissions, ct))
                        return Result.DATA_NOT_CREATE;

                    clearPermissionCache = true;
                }

                if (toDelete.IsNotEmpty())
                {
                    if (!await _repoPermission.BulkDeleteAsync(toDelete, ct))
                        return Result.DATA_NOT_DELETE;

                    clearPermissionCache = true;
                }

                var adminRole = await _repoRole.Query()
                    .FirstOrDefaultAsync(x => x.Name == RoleConst.Admin, ct);

                var userAdminIds = new List<string>();
                if (adminRole != null)
                {
                    if (toDelete.IsNotEmpty())
                    {
                        var toDeleteIds = toDelete.Select(p => p.Id).ToList();
                        var adminMappingsToDelete = await _repoRolePermission.Query()
                            .Where(rp => rp.RoleId == adminRole.Id && toDeleteIds.Contains(rp.PermissionId))
                            .ToListAsync(ct);

                        if (adminMappingsToDelete.IsNotEmpty())
                        {
                            if (!await _repoRolePermission.BulkDeleteAsync(adminMappingsToDelete, ct))
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

                        if (!await _repoRolePermission.BulkInsertAsync(rolePermissions, ct))
                            return Result.DATA_NOT_CREATE;

                        clearPermissionCache = true;
                    }

                    if (clearPermissionCache)
                    {
                        userAdminIds = await _repoUserRole.Query(true).Where(x => x.RoleId == adminRole.Id).Select(x => x.UserId).ToListAsync(ct);
                    }
                }

                await _uow.CommitAsync(ct);

                if (clearPermissionCache && userAdminIds.IsNotEmpty())
                {
                    var cacheItems = userAdminIds.SelectDistinctToList(id => CacheSettings.Permissions(id));

                    await _cache.RemoveAsync(cacheItems);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserIdAsync(GetUnassignedPermissionsByUserIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
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
                    .ToListAsync(ct);

                var models = permissions.IsNotEmpty()
                    ? permissions.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result> AssignPermissionsAsync(AssignPermissionsRequest request, CancellationToken ct = default)
        {
            var validation = request.Validate();
            if (!validation.IsValid)
            {
                var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }
            if (request.PermissionIds.IsEmpty())
            {
                var msg = "Danh sách quyền không được để trống";
                _logger.Debug(msg);
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            var currentUserId = _userContext.GetUserId();
            var guardResult = await _permissionGuard.ValidateAsync(request.TargetType, request.ObjectId, currentUserId);
            if (guardResult.IsError())
            {
                _logger.Debug(guardResult.Message);
                return guardResult;
            }

            var rs = new Result();
            switch (request.TargetType)
            {
                case PermissionTargetType.User:
                    rs = await AssignPermissionsToUserAsync(request);
                    break;
                case PermissionTargetType.Role:
                    rs = await AssignPermissionsToRoleAsync(request);
                    break;
                default:
                    var msg = "Loại đối tượng không hợp lệ";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            return rs;
        }

        public async Task<Result> UnassignPermissionsAsync(UnassignPermissionsRequest request, CancellationToken ct = default)
        {
            var validation = request.Validate();
            if (!validation.IsValid)
            {
                var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }
            if (request.PermissionIds.IsEmpty())
            {
                var msg = "Danh sách quyền không được để trống";
                _logger.Debug(msg);
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            var currentUserId = _userContext.GetUserId();
            var guardResult = await _permissionGuard.ValidateAsync(request.TargetType, request.ObjectId, currentUserId);
            if (guardResult.IsError())
            {
                _logger.Debug(guardResult.Message);
                return guardResult;
            }

            var rs = new Result();
            switch (request.TargetType)
            {
                case PermissionTargetType.User:
                    rs = await UnassignPermissionsFromUserAsync(request);
                    break;
                case PermissionTargetType.Role:
                    rs = await UnassignPermissionsFromRoleAsync(request);
                    break;
                default:
                    var msg = "Loại đối tượng không hợp lệ";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            return rs;
        }

        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByRoleIdAsync(GetAssignedPermissionsByRoleIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }

                var cacheItem = CacheSettings.RoleAssignedPermissions(request.RoleId);
                var cached = await _cache.TryGetValueAsync<List<PermissionModel>>(cacheItem);
                if (cached != null)
                    return Result.Ok(cached);

                var entities = await _repoRolePermission.Query(true)
                    .Where(rp => rp.RoleId == request.RoleId)
                    .Join(
                        _repoPermission.Query(true),
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p
                    )
                    .ToListAsync(ct);

                var models = entities.IsNotEmpty()
                    ? entities.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                await _cache.SetAsync(models, cacheItem);
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByRoleIdAsync(GetUnassignedPermissionsByRoleIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }

                var cacheItem = CacheSettings.RoleUnassignedPermissions(request.RoleId);
                var cached = await _cache.TryGetValueAsync<List<PermissionModel>>(cacheItem);
                if (cached != null)
                    return Result.Ok(cached);

                var entities = await _repoPermission.Query(true)
                    .Where(p =>
                        !_repoRolePermission.Query(true)
                            .Any(rp => rp.RoleId == request.RoleId && rp.PermissionId == p.Id)
                    )
                    .ToListAsync(ct);

                var models = entities.IsNotEmpty()
                    ? entities.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                await _cache.SetAsync(models, cacheItem);
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        private async Task<Result> AssignPermissionsToRoleAsync(AssignPermissionsRequest request)
        {
            try
            {
                var role = await _repoRole.FirstOrDefaultAsync(r => r.Id == request.ObjectId && !r.IsDeleted);
                if (role == null)
                {
                    var msg = "Không tìm thấy role";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }
                var inputPermissionIds = request.PermissionIds.Distinct().ToHashSet();
                var existedPermissionIds = await _repoRolePermission
                    .Query()
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();
                var existedSet = existedPermissionIds.ToHashSet();

                // chỉ insert những permission chưa tồn tại
                var toInsert = inputPermissionIds
                    .Except(existedSet)
                    .Select(pid => new RolePermissionEntity
                    {
                        RoleId = role.Id,
                        PermissionId = pid
                    })
                    .ToList();

                if (toInsert.IsNotEmpty())
                {
                    var inserted = await _repoRolePermission.BulkInsertAsync(toInsert);
                    if (!inserted)
                    {
                        var msg = "Gán quyền cho role không thành công";
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }

                }

                // clear cache của toàn bộ user thuộc role này + role permission cache
                var affectedUserIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var roleCacheItems = new List<CacheItem>
                {
                    CacheSettings.RoleAssignedPermissions(role.Id),
                    CacheSettings.RoleUnassignedPermissions(role.Id)
                };
                await _cache.RemoveAsync(roleCacheItems);

                if (affectedUserIds.IsNotEmpty())
                {
                    var cacheItems = affectedUserIds.SelectDistinctToList(id => CacheSettings.Permissions(id));
                    await _cache.RemoveAsync(cacheItems);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Lỗi khi gán quyền vào role";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task<Result> AssignPermissionsToUserAsync(AssignPermissionsRequest request)
        {
            try
            {
                var userExists = await _repoUser.FirstOrDefaultAsync(x => x.Id == request.ObjectId && x.IsDeleted == false);
                if (userExists == null)
                {
                    var msg = "Không tìm thấy người dùng cần gán quyền";
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var inputPermissionSet = request.PermissionIds
                    .Distinct()
                    .ToHashSet();

                var rolePermissionIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.UserId == request.ObjectId)
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
                    .Where(up => up.UserId == request.ObjectId)
                    .ToListAsync();

                var userPermissionDict = userPermissions
                    .ToDictionary(x => x.PermissionId);

                var toInsert = new List<UserPermissionEntity>();
                var toUpdate = new List<UserPermissionEntity>();

                foreach (var permId in inputPermissionSet)
                {
                    if (userPermissionDict.TryGetValue(permId, out var existingUp))
                    {
                        if (existingUp.Status == UserPermissionStatus.Deny)
                        {
                            existingUp.Status = UserPermissionStatus.Grant;
                            toUpdate.Add(existingUp);
                        }
                    }
                    else
                    {
                        toInsert.Add(new UserPermissionEntity
                        {
                            UserId = request.ObjectId,
                            PermissionId = permId,
                            Status = UserPermissionStatus.Grant
                        });
                    }
                }

                if (toInsert.IsNotEmpty())
                {
                    if (!await _repoUserPermission.BulkInsertAsync(toInsert))
                    {
                        var msg = "Thêm quyền cho người dùng thất bại";
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }
                }

                if (toUpdate.IsNotEmpty())
                {
                    if (!await _repoUserPermission.BulkUpdateAsync(toUpdate))
                    {
                        var msg = "Cập nhật quyền cho người dùng thất bại";
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                    }
                }

                var cacheItem = CacheSettings.Permissions(request.ObjectId);
                await _cache.RemoveAsync(cacheItem);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception(msg, ex);
            }
        }

        private async Task<Result> UnassignPermissionsFromRoleAsync(UnassignPermissionsRequest request)
        {
            try
            {
                var role = await _repoRole.FirstOrDefaultAsync(r => r.Id == request.ObjectId && !r.IsDeleted);
                if (role == null)
                {
                    var msg = "Không tìm thấy role";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var rolePermissions = await _repoRolePermission.Query()
                    .Where(rp =>
                        rp.RoleId == request.ObjectId &&
                        request.PermissionIds.Contains(rp.PermissionId)
                    )
                    .ToListAsync();

                if (rolePermissions.IsEmpty())
                {
                    return Result.Ok();
                }

                var deleted = await _repoRolePermission.BulkDeleteAsync(rolePermissions);
                if (!deleted)
                {
                    var msg = "Gỡ quyền khỏi role thất bại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                var userIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.RoleId == request.ObjectId)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var roleCacheItems = new List<CacheItem>
                {
                    CacheSettings.RoleAssignedPermissions(request.ObjectId),
                    CacheSettings.RoleUnassignedPermissions(request.ObjectId)
                };
                await _cache.RemoveAsync(roleCacheItems);

                if (userIds.IsNotEmpty())
                {
                    var cacheKeys = userIds.SelectDistinctToList(uid => CacheSettings.Permissions(uid));
                    await _cache.RemoveAsync(cacheKeys);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Lỗi khi gỡ quyền khỏi role";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task<Result> UnassignPermissionsFromUserAsync(UnassignPermissionsRequest request)
        {
            try
            {
                var userExists = await _repoUser.FirstOrDefaultAsync(x => x.Id == request.ObjectId && x.IsDeleted == false);
                if (userExists == null)
                {
                    var msg = "Không tìm thấy người dùng cần gỡ quyền";
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var inputPermissionSet = request.PermissionIds.Distinct().ToHashSet();

                var rolePermissionIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.UserId == request.ObjectId)
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
                    .Where(up => up.UserId == request.ObjectId && inputPermissionSet.Contains(up.PermissionId))
                    .ToListAsync();

                var userPermissionDict = userPermissions.ToDictionary(x => x.PermissionId);

                var toInsert = new List<UserPermissionEntity>();
                var toUpdate = new List<UserPermissionEntity>();
                var toDelete = new List<UserPermissionEntity>();

                foreach (var permId in inputPermissionSet)
                {
                    var isFromRole = rolePermissionSet.Contains(permId);

                    if (userPermissionDict.TryGetValue(permId, out var existingUp))
                    {
                        if (isFromRole)
                        {
                            if (existingUp.Status == UserPermissionStatus.Grant)
                            {
                                existingUp.Status = UserPermissionStatus.Deny;
                                toUpdate.Add(existingUp);
                            }
                        }
                        else
                        {
                            toDelete.Add(existingUp);
                        }
                    }
                    else
                    {
                        if (isFromRole)
                        {
                            toInsert.Add(new UserPermissionEntity
                            {
                                UserId = request.ObjectId,
                                PermissionId = permId,
                                Status = UserPermissionStatus.Deny
                            });
                        }
                    }
                }

                if (toDelete.IsNotEmpty())
                {
                    if (!await _repoUserPermission.BulkDeleteAsync(toDelete))
                    {
                        var msg = "Gỡ quyền khỏi người dùng thất bại";
                        return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                    }
                }

                if (toInsert.IsNotEmpty())
                {
                    if (!await _repoUserPermission.BulkInsertAsync(toInsert))
                    {
                        var msg = "Ghi đè quyền của người dùng thất bại";
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }
                }

                if (toUpdate.IsNotEmpty())
                {
                    if (!await _repoUserPermission.BulkUpdateAsync(toUpdate))
                    {
                        var msg = "Cập nhật quyền của người dùng thất bại";
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                    }
                }

                var cacheItem = CacheSettings.Permissions(request.ObjectId);
                await _cache.RemoveAsync(cacheItem);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                return Result.Exception(msg, ex);
            }
        }
    }
}
