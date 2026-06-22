using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Services;

namespace ManageLife.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddApplicationCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IUserTelegramConnectionService, UserTelegramConnectionService>();
            services.AddScoped<ITelegramBotCommandService, TelegramBotCommandService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ITranslationContext, TranslationContext>();
            services.AddScoped<ILanguageContext, LanguageContext>();
            services.AddScoped<IExceptionItemService, ExceptionItemService>();
            services.AddScoped<ITelegramService, TelegramService>();
            services.AddScoped<IUtilityService, UtilityService>();
            services.AddScoped<IQrService, QrService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ITodoTaskService, TodoTaskService>();
            services.AddScoped<ITodoListService, TodoListService>();
            services.AddScoped<ITodoReminderService, TodoReminderService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionGuard, PermissionGuard>();
            services.AddScoped<ITranslationFileService, TranslationFileService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ICronJobService, CronJobService>();
            services.AddScoped<ITelegramFileService, TelegramFileService>();
            services.AddSingleton<ITelegramUploadQueue, TelegramUploadQueue>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddSingleton<YtDlpManager>();
            services.AddScoped<IVideoDownloaderService, VideoDownloaderService>();
            services.AddScoped<IDictionaryApiService, DictionaryApiService>();
            services.AddScoped<IVocabWordService, VocabWordService>();
            services.AddScoped<IVocabTopicService, VocabTopicService>();
            services.AddScoped<IVocabDeckService, VocabDeckService>();
            services.AddScoped<IVocabStudyService, VocabStudyService>();
            services.AddScoped<ISequentialCodeGenerator, SequentialCodeGenerator>();
            services.AddScoped<ICodeSequenceService, CodeSequenceService>();
            services.AddScoped<IShortUrlService, ShortUrlService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<INoteTagService, NoteTagService>();
            services.AddScoped<IHabitService, HabitService>();
            services.AddScoped<IPomodoroService, PomodoroService>();
            services.AddScoped<ISettingContext, SettingContext>();
            return services;
        }
    }
}
