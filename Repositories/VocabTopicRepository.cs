using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class VocabTopicRepository : RepositoryBase<VocabTopicEntity>, IVocabTopicRepository
    {
        public VocabTopicRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
