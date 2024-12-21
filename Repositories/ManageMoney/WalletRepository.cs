using ManageLife.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Repositories
{
    public class WalletRepository : BaseRepository
    {
        public WalletRepository(DbContext context) : base(context)
        {

        }
    }
}
