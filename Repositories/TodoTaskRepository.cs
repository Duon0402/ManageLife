using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TodoTaskRepository : RepositoryBase<TodoTaskEntity>, ITodoTaskRepository
    {
        public TodoTaskRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
