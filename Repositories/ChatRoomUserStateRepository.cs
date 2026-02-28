using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ChatRoomUserStateRepository : RepositoryBase<ChatRoomUserStateEntity>, IChatRoomUserStateRepository
    {
        public ChatRoomUserStateRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
