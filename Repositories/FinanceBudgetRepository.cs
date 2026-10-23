using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceBudgetRepository : RepositoryBase<FinanceBudgetEntity>, IFinanceBudgetRepository
    {
        public FinanceBudgetRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
