using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ExceptionItemRepository : RepositoryBase<ExceptionItemEntity>, IExceptionItemRepository
    {
        public ExceptionItemRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}

