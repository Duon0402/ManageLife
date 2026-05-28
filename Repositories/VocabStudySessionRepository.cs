using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class VocabStudySessionRepository : RepositoryBase<VocabStudySessionEntity>, IVocabStudySessionRepository
    {
        public VocabStudySessionRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
