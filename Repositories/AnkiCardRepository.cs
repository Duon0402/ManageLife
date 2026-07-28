using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class AnkiCardRepository : RepositoryBase<AnkiCardEntity>, IAnkiCardRepository
    {
        public AnkiCardRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext) { }
    }
}
