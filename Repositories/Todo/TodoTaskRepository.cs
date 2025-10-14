using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class TodoTaskRepository : RepositoryBase<TodoTaskEntity>
    {
        public TodoTaskRepository(AppDbContext context) : base(context)
        {
        }
    }
}
