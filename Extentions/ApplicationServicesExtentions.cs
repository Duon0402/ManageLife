using ManageLife.Data;
using ManageLife.Helpers;
using ManageLife.Repositories;
using ManageLife.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ManageLife.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            #region DependencyInjection
            services.AddScoped<UserService>();
            services.AddScoped<UserRepository>();
            services.AddScoped<RoleRepository>();
            services.AddScoped<UserRoleRepository>();
            services.AddScoped<UserRefreshTokenRepository>();
            services.AddScoped<IMenuRegister, MenuRegister>();
            #endregion

            #region JWT Authentication
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
            #endregion

            #region AddAutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfiles));
            #endregion

            #region AddDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = config.GetConnectionString("DefaultConnection") ?? "";
                var serverVersion = new MySqlServerVersion(new Version(5, 2, 1));

                options.UseMySql(connectionString, serverVersion, mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                });
            });
            #endregion

            services.AddHttpContextAccessor();

            return services;
        }
    }

}
