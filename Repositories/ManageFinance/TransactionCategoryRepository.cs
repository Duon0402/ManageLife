using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities.ManageFinance;

namespace ManageLife.Repositories
{
    public class TransactionCategoryRepository : RepositoryBase<TransactionCategoryEntity>
    {
        public TransactionCategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
