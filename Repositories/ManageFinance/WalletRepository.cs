using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
	public class WalletRepository : RepositoryBase<WalletEntity>, IReposiotyBase<WalletEntity>
    {
        public WalletRepository(AppDbContext context) : base(context)
        {
        }
    }
}
