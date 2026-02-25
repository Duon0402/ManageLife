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
        public DbSet<ExceptionItemEntity> ExceptionItems { get; set; } = default!;
        public DbSet<SettingEntity> Settings { get; set; } = default!;
        public DbSet<TranslationEntity> Translations { get; set; } = default!;
        public DbSet<LanguageEntity> Languages { get; set; } = default!;
        public DbSet<FileEntity> Files { get; set; } = default!;
        public DbSet<UserEntity> Users { get; set; } = default!;
        public DbSet<RoleEntity> Roles { get; set; } = default!;
        public DbSet<PermissionEntity> Permissions { get; set; } = default!;
        public DbSet<UserRoleEntity> UserRoles { get; set; } = default!;
        public DbSet<RolePermissionEntity> RolePermissions { get; set; } = default!;
        public DbSet<UserPermissionEntity> UserPermissions { get; set; } = default!;
        public DbSet<UserRefreshTokenEntity> UserRefreshTokens { get; set; } = default!;
        public DbSet<TodoListEntity> TodoLists { get; set; } = default!;
        public DbSet<TodoTaskEntity> TodoTasks { get; set; } = default!;
        public DbSet<UserTelegramConnectionEntity> UserTelegramConnections { get; set; } = default!;
        public DbSet<FolderEntity> Folders { get; set; } = default!;
        public DbSet<FolderFileEntity> FolderFiles { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolePermissionEntity>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<UserPermissionEntity>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<UserRoleEntity>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<FolderFileEntity>()
                .HasKey(ff => new { ff.FolderId, ff.FileId });
        }
    }
}