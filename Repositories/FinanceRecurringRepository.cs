using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceRecurringRepository : RepositoryBase<FinanceRecurringEntity>, IFinanceRecurringRepository
    {
        public FinanceRecurringRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
