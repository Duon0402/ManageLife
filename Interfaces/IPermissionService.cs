using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<List<PermissionModel>>> GetListPermissionsAsync();
        Task<Result<List<PermissionModel>>> GetAssignedPermissionsByUserIdAsync(GetAssignedPermissionsByUserIdRequest request);
        Task<Result<List<PermissionModel>>> GetUnassignedPermissionsByUserIdAsync(GetUnassignedPermissionsByUserIdRequest request);
        Task<Result> SyncPermissionsAsync(List<string> permissionCodes);
    }
}
