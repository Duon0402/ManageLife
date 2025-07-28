using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class UserRefreshTokenRepository : RepositoryBase<UserRefreshTokenEntity>
    {
        public UserRefreshTokenRepository(AppDbContext context) : base(context)
        {
        }
    }
}
