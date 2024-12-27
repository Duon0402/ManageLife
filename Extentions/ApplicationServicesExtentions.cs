using ManageLife.Data;
using ManageLife.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {

            #region // AddAutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles));
            #endregion

            #region // AddDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = config.GetConnectionString("DefaultConnection");
                var serverVersion = new MySqlServerVersion(new Version(5, 2, 1));

                options.UseMySql(connectionString, serverVersion, mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                });
            });
            #endregion

            return services;
        }
    }

}
