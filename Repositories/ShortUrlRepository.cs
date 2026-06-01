using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class ShortUrlRepository : RepositoryBase<ShortUrlEntity>, IShortUrlRepository
    {
        public ShortUrlRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
