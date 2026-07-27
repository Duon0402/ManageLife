using ManageLife.Settings;

namespace ManageLife.Extensions
{
    public static class AppOptionsExtensions
    {
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<TelegramOptions>(config.GetSection(TelegramOptions.Section));
            services.Configure<VideoDownloaderOptions>(config.GetSection(VideoDownloaderOptions.Section));
            services.Configure<CronJobOptions>(config.GetSection(CronJobOptions.Section));
            services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
            services.Configure<RedisOptions>(config.GetSection(RedisOptions.Section));
            return services;
        }
    }
}
