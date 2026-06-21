using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ExceptionItemRepository : RepositoryBase<ExceptionItemEntity>, IExceptionItemRepository
    {
        public ExceptionItemRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}

