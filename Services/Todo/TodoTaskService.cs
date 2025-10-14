using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;

namespace ManageLife.Services
{
    public class TodoTaskService : ServiceBase, ITodoTaskService
    {
        public TodoTaskService(AppDbContext context) : base(context)
        {
        }
    }
}
