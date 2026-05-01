using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<List<PermissionModel>>> GetListPermissionsAsync(CancellationToken ct = default);
        Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserIdAsync(GetAssignedPermissionsByUserIdRequest request, CancellationToken ct = default);
        Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserIdAsync(GetUnassignedPermissionsByUserIdRequest request, CancellationToken ct = default);
        Task<Result> AssignPermissionsAsync(AssignPermissionsRequest request, CancellationToken ct = default);
        Task<Result> UnassignPermissionsAsync(UnassignPermissionsRequest request, CancellationToken ct = default);
        Task<Result> SyncPermissionsAsync(List<string> permissionCodes, CancellationToken ct = default);
        Task<Result<List<PermissionModel>>> GetAssignedPermissionsByRoleIdAsync(GetAssignedPermissionsByRoleIdRequest request, CancellationToken ct = default);
        Task<Result<List<PermissionModel>>> GetUnAssignedPermissionsByRoleIdAsync(GetUnAssignedPermissionsByRoleIdRequest request, CancellationToken ct = default);
    }
}
