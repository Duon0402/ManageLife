using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserPermissionRepository : RepositoryBase<UserPermissionEntity>, IUserPermissionRepository
    {
        public UserPermissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
