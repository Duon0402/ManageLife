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
        public DbSet<TranslationEntity> Translations { get; set; } = default!;
        public DbSet<LanguageEntity> Languages { get; set; } = default!;
        public DbSet<FileEntity> Files { get; set; } = default!;
        public DbSet<UserEntity> Users { get; set; } = default!;
        public DbSet<RoleEntity> Roles { get; set; } = default!;
        public DbSet<UserRoleEntity> UserRoles { get; set; } = default!;
        public DbSet<UserRefreshTokenEntity> UserRefreshTokens { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //TODO: Tạo ra config/register riêng cho từng entity để tránh viết chung vào khó kiểm soát

            modelBuilder.Entity<LanguageEntity>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<TranslationEntity>()
                .HasIndex(x => new { x.Key, x.LanguageId })
                .IsUnique();

            modelBuilder.Entity<TranslationEntity>()
                .HasOne(t => t.Language)
                .WithMany(l => l.Translations)
                .HasForeignKey(t => t.LanguageId);

            modelBuilder.Entity<UserRoleEntity>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRoleEntity>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRoleEntity>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}