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
        public DbSet<PomodoroSessionEntity> PomodoroSessions { get; set; } = default!;
        public DbSet<PomodoroSettingEntity> PomodoroSettings { get; set; } = default!;
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
        public DbSet<ShortUrlEntity> ShortUrls { get; set; } = default!;
        public DbSet<ShortUrlClickEntity> ShortUrlClicks { get; set; } = default!;
        public DbSet<CodeSequenceEntity> CodeSequences { get; set; } = default!;
        public DbSet<NoteEntity> Notes { get; set; } = default!;
        public DbSet<NoteTagEntity> NoteTags { get; set; } = default!;
        public DbSet<NoteTagRelationEntity> NoteTagRelations { get; set; } = default!;
        public DbSet<NoteLinkEntity> NoteLinks { get; set; } = default!;
        public DbSet<HabitEntity> Habits { get; set; } = default!;
        public DbSet<TodoListEntity> TodoLists { get; set; } = default!;
        public DbSet<TodoTaskEntity> TodoTasks { get; set; } = default!;
        public DbSet<TodoChecklistItemEntity> TodoChecklistItems { get; set; } = default!;
        public DbSet<AnkiCardEntity> AnkiCards { get; set; } = default!;

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

            builder.Entity<TranslationEntity>()
                .HasIndex(x => new { x.LanguageId, x.IsDeleted });

            builder.Entity<UserTelegramConnectionEntity>()
                .HasIndex(x => new { x.UserId, x.IsDeleted });

            builder.Entity<HabitEntity>()
                .HasIndex(x => new { x.OwnerId, x.IsDeleted });

            ConfigureTodo(builder);

            builder.Entity<VocabDeckEntity>()
                .HasIndex(x => x.TopicId);

            builder.Entity<VocabDeckWordEntity>()
                .HasKey(x => new { x.DeckId, x.WordId });

            builder.Entity<VocabStudyProgressEntity>()
                .HasIndex(x => new { x.UserId, x.NextReviewDate });

            builder.Entity<VocabWordEntity>()
                .HasIndex(x => new { x.OwnerId, x.Word, x.IsDeleted });

            builder.Entity<VocabStudySessionEntity>()
                .HasIndex(x => new { x.UserId, x.StartedAt });

            builder.Entity<CodeSequenceEntity>()
                .HasIndex(x => x.Category)
                .IsUnique();

            builder.Entity<ShortUrlEntity>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<ShortUrlEntity>()
                .HasIndex(x => new { x.OwnerId, x.IsDeleted });

            builder.Entity<ShortUrlClickEntity>()
                .HasIndex(x => new { x.ShortUrlId, x.CreatedTime });

            builder.Entity<NoteTagRelationEntity>()
                .HasKey(x => new { x.NoteId, x.TagId });

            builder.Entity<NoteLinkEntity>()
                .HasKey(x => new { x.SourceNoteId, x.TargetNoteId });

            builder.Entity<NoteEntity>()
                .HasIndex(x => new { x.OwnerId, x.IsDeleted });

            builder.Entity<NoteTagEntity>()
                .HasIndex(x => new { x.OwnerId, x.IsDeleted });

            builder.Entity<NoteLinkEntity>()
                .HasIndex(x => x.SourceNoteId);

            builder.Entity<NoteLinkEntity>()
                .HasIndex(x => x.TargetNoteId);

            builder.Entity<PomodoroSettingEntity>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            builder.Entity<PomodoroSessionEntity>()
                .HasIndex(x => new { x.UserId, x.StartedAt });

            builder.Entity<AnkiCardEntity>()
                .HasIndex(x => new { x.OwnerId, x.IsDeleted });
        }

        private static void ConfigureTodo(ModelBuilder builder)
        {
            // Cột id dùng varchar(255) cho khớp EntityBase.Id (FK cần cùng kiểu) và index được trên MySQL
            builder.Entity<TodoListEntity>(e =>
            {
                e.Property(x => x.OwnerId).HasMaxLength(255);
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Color).HasMaxLength(9);
                e.Property(x => x.Icon).HasMaxLength(50);
                e.HasIndex(x => new { x.OwnerId, x.IsDeleted, x.SortOrder });
            });

            builder.Entity<TodoTaskEntity>(e =>
            {
                e.Property(x => x.OwnerId).HasMaxLength(255);
                e.Property(x => x.ListId).HasMaxLength(255);
                e.Property(x => x.Title).HasMaxLength(500);
                e.Property(x => x.Note).HasColumnType("text");
                e.HasOne<TodoListEntity>().WithMany().HasForeignKey(x => x.ListId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.OwnerId, x.IsDeleted, x.CompletedAt, x.DueDate });
                e.HasIndex(x => new { x.OwnerId, x.ListId, x.IsDeleted, x.SortOrder });
            });

            builder.Entity<TodoChecklistItemEntity>(e =>
            {
                e.Property(x => x.TaskId).HasMaxLength(255);
                e.Property(x => x.Title).HasMaxLength(500);
                e.HasOne<TodoTaskEntity>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.TaskId, x.SortOrder });
            });
        }
    }
}
