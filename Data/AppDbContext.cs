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
        public DbSet<TelegramBotCommandEntity> TelegramBotCommands { get; set; } = default!;
        public DbSet<FolderEntity> Folders { get; set; } = default!;
        public DbSet<FolderFileEntity> FolderFiles { get; set; } = default!;
        public DbSet<ChatMessageEntity> ChatMessages { get; set; } = default!;
        public DbSet<ChatRoomMemberEntity> ChatRoomMembers { get; set; } = default!;
        public DbSet<ChatRoomEntity> ChatRooms { get; set; } = default!;
        public DbSet<ChatRoomUserStateEntity> ChatRoomUserStates { get; set; } = default!;
        public DbSet<VocabTopicEntity> VocabTopics { get; set; } = default!;
        public DbSet<VocabDeckEntity> VocabDecks { get; set; } = default!;
        public DbSet<VocabWordEntity> VocabWords { get; set; } = default!;
        public DbSet<VocabDeckWordEntity> VocabDeckWords { get; set; } = default!;
        public DbSet<VocabStudyProgressEntity> VocabStudyProgress { get; set; } = default!;
        public DbSet<VocabStudySessionEntity> VocabStudySessions { get; set; } = default!;

        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RolePermissionEntity>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<UserPermissionEntity>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            builder.Entity<UserRoleEntity>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<FolderFileEntity>()
                .HasKey(ff => new { ff.FolderId, ff.FileId });

            builder.Entity<ChatRoomMemberEntity>()
                .HasIndex(x => x.UserId);

            builder.Entity<ChatMessageEntity>()
                .HasIndex(x => new { x.RoomId, x.CreatedTime });

            builder.Entity<ChatRoomEntity>()
                .HasIndex(x => x.PrivateKey)
                .IsUnique()
                .HasFilter("[PrivateKey] IS NOT NULL");

            builder.Entity<ChatRoomMemberEntity>()
                .HasKey(x => new { x.RoomId, x.UserId });

            builder.Entity<ChatRoomUserStateEntity>()
                .HasKey(x => new { x.RoomId, x.UserId });

            builder.Entity<UserRefreshTokenEntity>()
                .HasIndex(x => x.RefreshToken);

            builder.Entity<UserRefreshTokenEntity>()
                .HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiryTime });

            builder.Entity<TranslationEntity>()
                .HasIndex(x => x.LanguageId);

            builder.Entity<VocabDeckWordEntity>()
                .HasKey(x => new { x.DeckId, x.WordId });

            builder.Entity<VocabStudyProgressEntity>()
                .HasIndex(x => new { x.UserId, x.NextReviewDate });

            builder.Entity<VocabWordEntity>()
                .HasIndex(x => new { x.OwnerId, x.Word, x.IsDeleted });

            builder.Entity<VocabStudySessionEntity>()
                .HasIndex(x => new { x.UserId, x.StartedAt });
        }
    }
}