using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class NoteLinkRepository : RepositoryBase<NoteLinkEntity>, INoteLinkRepository
    {
        public NoteLinkRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext) { }
    }
}
