using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TodoListRepository : RepositoryBase<TodoListEntity>, ITodoListRepository
    {
        public TodoListRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
