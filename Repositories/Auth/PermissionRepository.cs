using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class PermissionRepository : RepositoryBase<PermissionEntity>, IPermissionRepository
    {
        public PermissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
