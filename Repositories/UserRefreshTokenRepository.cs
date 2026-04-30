using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserRefreshTokenRepository : RepositoryBase<UserRefreshTokenEntity>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
