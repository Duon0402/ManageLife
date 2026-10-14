using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FinanceCategoryRepository : RepositoryBase<FinanceCategoryEntity>, IFinanceCategoryRepository
    {
        public FinanceCategoryRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
