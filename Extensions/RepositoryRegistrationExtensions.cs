using ManageLife.Interfaces;
using ManageLife.Repositories;

namespace ManageLife.Extensions
{
    public static class RepositoryRegistrationExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserTelegramConnectionRepository, UserTelegramConnectionRepository>();
            services.AddScoped<IExceptionItemRepository, ExceptionItemRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ITranslationRepository, TranslationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
            services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<ITodoListRepository, TodoListRepository>();
            services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<IFolderFileRepository, FolderFileRepository>();

            return services;
        }
    }
}
