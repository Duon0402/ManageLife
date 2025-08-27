using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class TranslationRepository : RepositoryBase<TranslationEntity>
    {
        public TranslationRepository(AppDbContext context) : base(context)
        {
        }
    }
}
