using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ChatRoomRepository : RepositoryBase<ChatRoomEntity>, IChatRoomRepository
    {
        public ChatRoomRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
