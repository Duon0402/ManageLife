using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceDebtRepository : RepositoryBase<FinanceDebtEntity>, IFinanceDebtRepository
    {
        public FinanceDebtRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
