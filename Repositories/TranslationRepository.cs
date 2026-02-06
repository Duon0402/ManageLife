using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class TranslationRepository : RepositoryBase<TranslationEntity>, ITranslationRepository
    {
        public TranslationRepository(AppDbContext context) : base(context)
        {
        }
    }
}
