using ManageLife.Interfaces;
using ManageLife.Repositories;

namespace ManageLife.Extensions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Base repositories
            services.AddScoped<IExceptionItemRepository, ExceptionItemRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ITranslationRepository, TranslationRepository>();

            // Auth repositories
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            // Todo repositories
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();

            return services;
        }
    }
}
