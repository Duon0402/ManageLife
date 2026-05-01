using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ChatRoomUserStateRepository : RepositoryBase<ChatRoomUserStateEntity>, IChatRoomUserStateRepository
    {
        public ChatRoomUserStateRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
