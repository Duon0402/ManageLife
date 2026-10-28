using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceSavingGoalEntryRepository : RepositoryBase<FinanceSavingGoalEntryEntity>, IFinanceSavingGoalEntryRepository
    {
        public FinanceSavingGoalEntryRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
