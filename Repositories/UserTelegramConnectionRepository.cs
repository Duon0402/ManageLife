using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserTelegramConnectionRepository : RepositoryBase<UserTelegramConnectionEntity>, IUserTelegramConnectionRepository
    {
        public UserTelegramConnectionRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
