using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class NoteTagRepository : RepositoryBase<NoteTagEntity>, INoteTagRepository
    {
        public NoteTagRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext) { }
    }
}
