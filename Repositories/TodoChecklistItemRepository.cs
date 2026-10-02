using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TodoChecklistItemRepository : RepositoryBase<TodoChecklistItemEntity>, ITodoChecklistItemRepository
    {
        public TodoChecklistItemRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
