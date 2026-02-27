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
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITranslationContext, TranslationContext>();
            services.AddScoped<ILanguageContext, LanguageContext>();
            services.AddScoped<IExceptionItemService, ExceptionItemService>();
            services.AddScoped<ITelegramService, TelegramService>();
            services.AddScoped<IUtilityService, UtilityService>();
            services.AddScoped<IQrService, QrService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ITodoTaskService, TodoTaskService>();
            services.AddScoped<ITodoListService, TodoListService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionGuard, PermissionGuard>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ICronJobService, CronJobService>();
            services.AddScoped<ITelegramFileService, TelegramFileService>();
            services.AddSingleton<ITelegramUploadQueue, TelegramUploadQueue>();
            services.AddScoped<IFolderService, FolderService>();
            return services;
        }
    }
}
