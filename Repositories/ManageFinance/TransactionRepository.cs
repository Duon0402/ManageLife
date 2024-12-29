using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class TransactionRepository : RepositoryBase<TransactionEntity>
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
