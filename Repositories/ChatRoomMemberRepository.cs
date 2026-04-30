using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ChatRoomMemberRepository : RepositoryBase<ChatRoomMemberEntity>, IChatRoomMemberRepository
    {
        public ChatRoomMemberRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
