using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
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
        private readonly RolePermissionRepository _repoRolePermission;

        public PermissionService(AppDbContext context, ICacheService cache) : base(context)
        {
            _cache = cache;
            _repoPermission = new PermissionRepository(context);
            _repoUserPermission = new UserPermissionRepository(context);
            _repoRolePermission = new RolePermissionRepository(context);
            _repoUserRole = new UserRoleRepository(context);
            _repoRole = new RoleRepository(context);
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

        public async Task<Result<List<PermissionModel>>> GetListPermissionsByUserIdAsync(GetListPermissionsByUserIdRequest request)
        {
            try
            {
                if (request == null || request.UserId.IsEmpty())
                {
                    return Result.Error<List<PermissionModel>>(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);
                }

                var cacheKeyItem = CacheKey.Permissions(request.UserId, TimeSpan.FromMinutes(30));

                var cachedPermissions = await _cache.TryGetValueAsync<List<PermissionModel>>(cacheKeyItem.Key);
                if (cachedPermissions.IsNotEmpty())
                {
                    return Result.Ok(cachedPermissions);
                }

                var userPermissions = await _repoUserPermission.Query(true)
                    .Where(up => up.UserId == request.UserId)
                    .Include(up => up.Permission)
                    .Select(x => x.Permission)
                    .ToListAsync();

                var rolePermissions = await _repoUserRole.Query(true)
                    .Where(ur => ur.UserId == request.UserId)
                    .Join(_repoRolePermission.Query(true),
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.Permission)
                    .ToListAsync();

                var allPermissions = userPermissions
                    .Union(rolePermissions)
                    .GroupBy(p => p!.Id)
                    .Select(g => g.First())
                    .ToList();

                var models = allPermissions.IsNotEmpty()
                    ? allPermissions!.MapToList<PermissionModel>()
                    : new List<PermissionModel>();

                await _cache.SetAsync(cacheKeyItem.Key, models, cacheKeyItem.Expiry);

                await _cache.RemoveAsync(CacheKey.MenuItems().Key);
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
            try
            {
                var dbPermissions = await _repoPermission.GetAllAsync();
                var dbPermissionCodes = dbPermissions.Select(p => p.Code).ToList();

                var toInsertCodes = permissionCodes.Except(dbPermissionCodes).ToList();
                var toDelete = dbPermissions.Where(p => !permissionCodes.Contains(p.Code)).ToList();

                var insertPermissions = toInsertCodes.Select(code => new PermissionEntity
                {
                    Id = IdHeper.NewId(),
                    Code = code,
                    Name = code,
                    CreatedUser = SystemUsers.System
                }).ToList();

                if (insertPermissions.Any())
                {
                    if (!await _repoPermission.BulkInsertAsync(insertPermissions, uow))
                    {
                        return Result.DATA_NOT_CREATE;
                    }
                }

                if (toDelete.Any())
                {
                    if (!await _repoPermission.BulkDeleteAsync(toDelete, uow))
                    {
                        return Result.DATA_NOT_DELETE;
                    }
                }

                var adminRole = await _repoRole.Query()
                    .FirstOrDefaultAsync(x => x.Name == RoleConst.Admin);

                if (adminRole != null)
                {
                    if (toDelete.Any())
                    {
                        var toDeleteIds = toDelete.Select(p => p.Id).ToList();
                        var adminMappingsToDelete = await _repoRolePermission.Query()
                            .Where(rp => rp.RoleId == adminRole.Id && toDeleteIds.Contains(rp.PermissionId))
                            .ToListAsync();

                        if (adminMappingsToDelete.Any())
                        {
                            if (!await _repoRolePermission.BulkDeleteAsync(adminMappingsToDelete, uow))
                            {
                                return Result.DATA_NOT_DELETE;
                            }
                        }
                    }

                    if (insertPermissions.Any())
                    {
                        var rolePermissions = insertPermissions.Select(p => new RolePermissionEntity
                        {
                            RoleId = adminRole.Id,
                            PermissionId = p.Id
                        }).ToList();

                        if (!await _repoRolePermission.BulkInsertAsync(rolePermissions, uow))
                        {
                            return Result.DATA_NOT_CREATE;
                        }
                    }
                }

                await uow.CommitAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }
    }
}
