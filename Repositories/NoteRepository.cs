using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class NoteRepository : RepositoryBase<NoteEntity>, INoteRepository
    {
        public NoteRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext) { }
    }
}
