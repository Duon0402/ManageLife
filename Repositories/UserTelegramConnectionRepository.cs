using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserTelegramConnectionRepository : RepositoryBase<UserTelegramConnectionEntity>, IUserTelegramConnectionRepository
    {
        public UserTelegramConnectionRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
