using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserRoleRepository : RepositoryBase<UserRoleEntity>, IUserRoleRepository
    {
        public UserRoleRepository(AppDbContext context) : base(context)
        {
        }
    }
}
