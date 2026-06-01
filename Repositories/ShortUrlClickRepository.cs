using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ShortUrlClickRepository : RepositoryBase<ShortUrlClickEntity>, IShortUrlClickRepository
    {
        public ShortUrlClickRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
