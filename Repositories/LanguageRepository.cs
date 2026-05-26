using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class LanguageRepository : RepositoryBase<LanguageEntity>, ILanguageRepository
    {
        public LanguageRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}


