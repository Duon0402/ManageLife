using ManageLife.Entities;
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
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
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
