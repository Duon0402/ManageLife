using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IRoleService
    {
        Task<Result<List<RoleModel>>> GetListRolesAsync();
        Task<Result<List<RoleModel>>> GetListRolesByUserIdAsync(GetListRolesByUserIdRequest request);
        Task<Result<RoleModel>> GetRoleByIdAsync(GetRoleByIdRequest request);
        Task<Result> CreateRoleAsync(CreateRoleRequest request);
        Task<Result> DeleteRoleAsync(DeleteRoleRequest request);
    }
}
