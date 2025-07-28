using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class UserRoleRepository : RepositoryBase<UserRoleEntity>
    {
        public UserRoleRepository(AppDbContext context) : base(context)
        {
        }
    }
}
