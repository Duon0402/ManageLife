using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class LanguageRepository : RepositoryBase<LanguageEntity>, ILanguageRepository
    {
        public LanguageRepository(AppDbContext context) : base(context)
        {
        }
    }
}

