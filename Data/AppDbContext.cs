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
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Language & Translation
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
            #endregion

            #region UserRole
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
            #endregion

            #region RolePermission
            modelBuilder.Entity<RolePermissionEntity>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermissionEntity>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermissionEntity>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
            #endregion

            #region UserPermission
            modelBuilder.Entity<UserPermissionEntity>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<UserPermissionEntity>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPermissionEntity>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);
            #endregion

            #region Todo
            modelBuilder.Entity<TodoListEntity>()
                .HasMany(l => l.Tasks)
                .WithOne(t => t.TodoList)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoTaskEntity>()
                .HasMany(t => t.SubTasks)
                .WithOne(st => st.ParentTask)
                .HasForeignKey(st => st.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region UserTelegramConnection

            modelBuilder.Entity<UserTelegramConnectionEntity>(entity =>
            {
                entity.ToTable("UserTelegramConnections");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.ChatId)
                    .IsUnique();

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<UserTelegramConnectionEntity>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.ChatId)
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .IsRequired();

            });

            #endregion
        }
    }
}