using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class NoteTagRelationRepository : RepositoryBase<NoteTagRelationEntity>, INoteTagRelationRepository
    {
        public NoteTagRelationRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext) { }
    }
}
