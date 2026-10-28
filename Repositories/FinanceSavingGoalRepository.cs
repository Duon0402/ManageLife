using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceSavingGoalRepository : RepositoryBase<FinanceSavingGoalEntity>, IFinanceSavingGoalRepository
    {
        public FinanceSavingGoalRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
