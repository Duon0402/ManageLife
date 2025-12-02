using ManageLife.Repositories;

namespace ManageLife.Extensions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ExceptionItemRepository>();
            services.AddScoped<SettingRepository>();
            services.AddScoped<TodoTaskRepository>();
            services.AddScoped<TodoListRepository>();
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
