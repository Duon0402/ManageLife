using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class VocabDeckWordRepository : RepositoryBase<VocabDeckWordEntity>, IVocabDeckWordRepository
    {
        public VocabDeckWordRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
