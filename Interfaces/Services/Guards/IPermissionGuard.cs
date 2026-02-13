using ManageLife.Base;
using ManageLife.Commons;

namespace ManageLife.Interfaces
{
    public interface IPermissionGuard
    {
        Task<Result> ValidateAsync(PermissionTargetType targetType, string targetObjectId, string? currentUserId);
    }
}
