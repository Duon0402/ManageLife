using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<List<PermissionModel>>> GetListPermissionsAsync();
    }
}
