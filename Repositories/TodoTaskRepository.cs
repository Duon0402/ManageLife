using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TodoTaskRepository : RepositoryBase<TodoTaskEntity>, ITodoTaskRepository
    {
        public TodoTaskRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
