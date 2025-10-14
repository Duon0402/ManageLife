using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class TodoListRepository : RepositoryBase<TodoListEntity>
    {
        public TodoListRepository(AppDbContext context) : base(context)
        {
        }
    }
}
