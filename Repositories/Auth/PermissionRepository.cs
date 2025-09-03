using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class PermissionRepository : RepositoryBase<PermissionEntity>
    {
        public PermissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
