using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class VocabDeckRepository : RepositoryBase<VocabDeckEntity>, IVocabDeckRepository
    {
        public VocabDeckRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
