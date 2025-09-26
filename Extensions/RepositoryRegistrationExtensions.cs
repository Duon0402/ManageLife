using ManageLife.Repositories;
using ManageLife.Services;

namespace ManageLife.Extensions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<PermissionService>();
            services.AddScoped<TranslationRepository>();
            services.AddScoped<LanguageRespository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<UserRoleRepository>();
            services.AddScoped<UserRefreshTokenRepository>();

            return services;
        }
    }
}
