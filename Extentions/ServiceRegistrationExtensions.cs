using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Services;

namespace ManageLife.Extentions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddApplicationCustomServices(this IServiceCollection services)
        {
            services.AddScoped<ITranslationService, TranslationService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ICronJobService, CronJobService>();
            services.AddScoped<TelegramService>();
            services.AddScoped<TelegramFileService>();
            services.AddScoped<UserService>();

            services.AddScoped<IMenuRegister, MenuRegister>();

            return services;
        }
    }
}
