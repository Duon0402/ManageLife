using ManageLife.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Repositories
{
    public class WalletRepository : RepositoryBase
    {
        public WalletRepository(DbContext context) : base(context)
        {

        }
    }
}
