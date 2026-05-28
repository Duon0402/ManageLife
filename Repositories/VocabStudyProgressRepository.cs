using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class VocabStudyProgressRepository : RepositoryBase<VocabStudyProgressEntity>, IVocabStudyProgressRepository
    {
        public VocabStudyProgressRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
