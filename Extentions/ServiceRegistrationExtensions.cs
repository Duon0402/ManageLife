using ManageLife.Interfaces;
using ManageLife.Services;

namespace ManageLife.Extentions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddApplicationCustomServices(this IServiceCollection services)
        {
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ICronJobService, CronJobService>();
            services.AddScoped<TelegramService>();
            services.AddScoped<TelegramFileService>();
            return services;
        }
    }
}
