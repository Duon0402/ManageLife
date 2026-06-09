using ManageLife.Interfaces;
using ManageLife.Repositories;

namespace ManageLife.Extensions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserTelegramConnectionRepository, UserTelegramConnectionRepository>();
            services.AddScoped<ITelegramBotCommandRepository, TelegramBotCommandRepository>();
            services.AddScoped<IExceptionItemRepository, ExceptionItemRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ITranslationRepository, TranslationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<IFolderFileRepository, FolderFileRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IChatRoomMemberRepository, ChatRoomMemberRepository>();
            services.AddScoped<IChatRoomUserStateRepository, ChatRoomUserStateRepository>();
            services.AddScoped<IVocabTopicRepository, VocabTopicRepository>();
            services.AddScoped<IVocabDeckRepository, VocabDeckRepository>();
            services.AddScoped<IVocabWordRepository, VocabWordRepository>();
            services.AddScoped<IVocabDeckWordRepository, VocabDeckWordRepository>();
            services.AddScoped<IVocabStudyProgressRepository, VocabStudyProgressRepository>();
            services.AddScoped<IVocabStudySessionRepository, VocabStudySessionRepository>();
            services.AddScoped<ICodeSequenceRepository, CodeSequenceRepository>();
            services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
            services.AddScoped<IShortUrlClickRepository, ShortUrlClickRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<INoteTagRepository, NoteTagRepository>();
            services.AddScoped<INoteTagRelationRepository, NoteTagRelationRepository>();
            services.AddScoped<INoteLinkRepository, NoteLinkRepository>();
            services.AddScoped<IHabitRepository, HabitRepository>();

            return services;
        }
    }
}
