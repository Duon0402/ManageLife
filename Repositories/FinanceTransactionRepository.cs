using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceTransactionRepository : RepositoryBase<FinanceTransactionEntity>, IFinanceTransactionRepository
    {
        public FinanceTransactionRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
