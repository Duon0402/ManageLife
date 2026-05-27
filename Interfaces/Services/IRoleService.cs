using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IRoleService
    {
        Task<Result<List<RoleModel>>> GetListRolesAsync(CancellationToken ct = default);
        Task<Result<List<RoleModel>>> GetListRolesByUserIdAsync(GetListRolesByUserIdRequest request, CancellationToken ct = default);
        Task<Result<RoleModel>> GetRoleByIdAsync(GetRoleByIdRequest request, CancellationToken ct = default);
        Task<Result> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
        Task<Result> UpdateRoleAsync(UpdateRoleRequest request, CancellationToken ct = default);
        Task<Result> DeleteRoleAsync(DeleteRoleRequest request, CancellationToken ct = default);
    }
}
