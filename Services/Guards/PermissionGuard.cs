using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class PermissionGuard : IPermissionGuard
    {
        private readonly IUserRoleRepository _repoUserRole;
        private readonly IAppLogger<PermissionGuard> _logger;

        public PermissionGuard(
            IUserRoleRepository repoUserRole,
            IAppLogger<PermissionGuard> logger)
        {
            _repoUserRole = repoUserRole;
            _logger = logger;
        }

        public async Task<Result> ValidateAsync(PermissionTargetType targetType, string targetObjectId, string? currentUserId)
        {
            if (targetType == PermissionTargetType.User && targetObjectId == currentUserId)
            {
                const string msg = "Không được thay đổi quyền của chính bạn";
                _logger.Debug(msg);
                return Result.Error("SECURITY", msg);
            }

            if (targetType == PermissionTargetType.Role)
            {
                var isSelfRole = await _repoUserRole.Query(true)
                    .AnyAsync(x =>
                        x.UserId == currentUserId &&
                        x.RoleId == targetObjectId);

                if (isSelfRole)
                {
                    const string msg = "Không được thay đổi quyền của role bạn đang thuộc";
                    _logger.Debug(msg);
                    return Result.Error("SECURITY", msg);
                }
            }

            return Result.Ok();
        }
    }
}
