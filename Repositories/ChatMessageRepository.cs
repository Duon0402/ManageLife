using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ChatMessageRepository : RepositoryBase<ChatMessageEntity>, IChatMessageRepository
    {
        public ChatMessageRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
