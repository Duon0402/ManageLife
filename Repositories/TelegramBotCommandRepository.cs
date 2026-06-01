using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TelegramBotCommandRepository : RepositoryBase<TelegramBotCommandEntity>, ITelegramBotCommandRepository
    {
        public TelegramBotCommandRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
