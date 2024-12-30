using ManageLife.Entities;
using ManageLife.Entities.ManageFinance;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _config;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        #region DbSet<>
        public DbSet<WalletEntity> Wallets { get; set; }
        public DbSet<TransactionEntity> Transactions { get; set; }
        public DbSet<TransactionCategoryEntity> TransactionCategories { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
