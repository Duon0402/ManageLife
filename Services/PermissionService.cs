using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Contexts;
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
        private readonly IAppLogger<PermissionService> _logger;
        private readonly IPermissionGuard _permissionGuard;
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
            IAppLogger<PermissionService> logger,
            IPermissionGuard permissionGuard,
            AppDbContext context)
        {
            _cache = cache;
            _repoPermission = repoPermission;
            _repoUserPermission = repoUserPermission;
            _repoRolePermission = repoRolePermission;
            _repoUserRole = repoUserRole;
            _repoRole = repoRole;
            _repoUser = repoUser;
            _logger = logger;
            _permissionGuard = permissionGuard;
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
            string msg;
            var validation = request.Validate();
            if (!validation.IsValid)
            {
                msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }
            if (request.PermissionIds.IsEmpty())
            {
                msg = "Danh sách quyền không được để trống";
                _logger.Debug(msg);
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            var currentUserId = UserContext.GetUserId();
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
                    msg = "Loại đối tượng không hợp lệ";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            return rs;
        }

        public async Task<Result> UnassignPermissionsAsync(UnassignPermissionsRequest request)
        {
            string msg;
            var validation = request.Validate();
            if (!validation.IsValid)
            {
                msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }
            if (request.PermissionIds.IsEmpty())
            {
                msg = "Danh sách quyền không được để trống";
                _logger.Debug(msg);
                return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            var currentUserId = UserContext.GetUserId();
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
                    msg = "Loại đối tượng không hợp lệ";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
            }

            return rs;
        }

        public async Task<Result<List<PermissionModel>>> GetAssignedPermissionsByRoleIdAsync(GetAssignedPermissionsByRoleIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }

                var entities = await _repoRolePermission.Query(true)
                    .Where(rp => rp.RoleId == request.RoleId)
                    .Join(
                        _repoPermission.Query(true),
                        rp => rp.PermissionId,
                        p => p.Id,
                        (rp, p) => p
                    )
                    .ToListAsync();

                if (entities.IsEmpty())
                {
                    return Result.Ok(new List<PermissionModel>());
                }

                var models = entities.MapToList<PermissionModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        public async Task<Result<List<PermissionModel>>> GetUnAssignedPermissionsByRoleIdAsync(GetUnAssignedPermissionsByRoleIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, msg);
                }
                var entities = await _repoPermission.Query(true)
                    .Where(p =>
                        !_repoRolePermission.Query(true)
                            .Any(rp => rp.RoleId == request.RoleId && rp.PermissionId == p.Id)
                    )
                    .ToListAsync();
                if (entities.IsEmpty())
                {
                    return Result.Ok(new List<PermissionModel>());
                }
                var models = entities.MapToList<PermissionModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }

        private async Task<Result> AssignPermissionsToRoleAsync(AssignPermissionsRequest request)
        {
            string msg;
            try
            {
                var role = await _repoRole.FirstOrDefaultAsync(r => r.Id == request.ObjectId && !r.IsDeleted);
                if (role == null)
                {
                    msg = "Không tìm thấy role";
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
                    var b = await _repoRolePermission.BulkInsertAsync(toInsert);
                    if (!b)
                    {
                        msg = "Gán quyền cho role không thành công";
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }

                }

                // clear cache của toàn bộ user thuộc role này
                var affectedUserIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                if (affectedUserIds.IsNotEmpty())
                {
                    var cacheItems = affectedUserIds.SelectDistinctToList(id => CacheSettings.Permissions(id));

                    await _cache.RemoveAsync(cacheItems);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Lỗi khi gán quyền vào role";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task<Result> AssignPermissionsToUserAsync(AssignPermissionsRequest request)
        {
            //TODO: Hoàn thành code phần gán quyền và gỡ quyền
            string msg;
            try
            {
                var userExists = await _repoUser.FirstOrDefaultAsync(x => x.Id == request.ObjectId && x.IsDeleted == false);
                if (userExists == null)
                {
                    msg = "Không tìm thấy người dùng cần gán quyền";
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

        private async Task<Result> UnassignPermissionsFromRoleAsync(UnassignPermissionsRequest request)
        {
            string msg;
            try
            {
                var role = await _repoRole.FirstOrDefaultAsync(r => r.Id == request.ObjectId && !r.IsDeleted);
                if (role == null)
                {
                    msg = "Không tìm thấy role";
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

                var b = await _repoRolePermission.BulkDeleteAsync(rolePermissions);
                if (!b)
                {
                    msg = "Gỡ quyền khỏi role thất bại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                var userIds = await _repoUserRole.Query(true)
                    .Where(ur => ur.RoleId == request.ObjectId)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                if (userIds.IsNotEmpty())
                {
                    var cacheKeys = userIds.SelectDistinctToList(uid => CacheSettings.Permissions(uid));

                    await _cache.RemoveAsync(cacheKeys);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = "Lỗi khi gỡ quyền khỏi role";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task<Result> UnassignPermissionsFromUserAsync(UnassignPermissionsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
