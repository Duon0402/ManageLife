using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class LanguageRespository : RepositoryBase<LanguageEntity>
    {
        public LanguageRespository(AppDbContext context) : base(context)
        {
        }
    }
}
