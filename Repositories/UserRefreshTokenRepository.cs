using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserRefreshTokenRepository : RepositoryBase<UserRefreshTokenEntity>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(AppDbContext context) : base(context)
        {
        }
    }
}
