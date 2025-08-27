using ManageLife.Repositories;

namespace ManageLife.Extentions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
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
