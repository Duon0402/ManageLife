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
		public DbSet<FileEntity> Files { get; set; } = default!;
		public DbSet<UserEntity> Users { get; set; } = default!;
		public DbSet<RoleEntity> Roles { get; set; } = default!;
		public DbSet<WalletEntity> Wallets { get; set; } = default!;
		public DbSet<TransactionEntity> Transactions { get; set; } = default!;
		public DbSet<TransactionCategoryEntity> TransactionCategories { get; set; } = default!;
		#endregion

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}
	}
}
