using ManageLife.Settings;

namespace ManageLife.Extensions
{
    public static class SettingsRegistrationExtensions
    {
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TelegramSettings>(config.GetSection(TelegramSettings.Section));
            services.Configure<VideoDownloaderSettings>(config.GetSection(VideoDownloaderSettings.Section));
            services.Configure<CronJobSettings>(config.GetSection(CronJobSettings.Section));
            services.Configure<JwtSettings>(config.GetSection(JwtSettings.Section));
            services.Configure<RedisSettings>(config.GetSection(RedisSettings.Section));
            return services;
        }
    }
}
