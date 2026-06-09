using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class HabitRepository : RepositoryBase<HabitEntity>, IHabitRepository
    {
        public HabitRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
