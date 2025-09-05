using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class RolePermissionRepository : RepositoryBase<RolePermissionEntity>
    {
        public RolePermissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
