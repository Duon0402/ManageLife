using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class UserPermissionRepository : RepositoryBase<UserPermissionEntity>
    {
        public UserPermissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
