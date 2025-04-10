using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class RoleRepository : RepositoryBase<RoleEntity>
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }
    }
}
