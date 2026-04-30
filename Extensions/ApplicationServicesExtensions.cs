using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Helpers;
using ManageLife.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using StackExchange.Redis;
using System.Text;

namespace ManageLife.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Set EPPlus license 1 lần
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            services.AddApplicationCustomServices();
            services.AddRepositories();
            services.AddHttpContextAccessor();
            services.AddScoped<IMenuRegister, MenuRegister>();
            services.AddHttpClient();

            #region JWT Authentication
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Cookies["accessToken"];
                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
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
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfiles>();
            });
            #endregion

            #region AddDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = config.GetConnectionString("DefaultConnection") ?? "";
                var serverVersion = ServerVersion.AutoDetect(connectionString);
                options.UseMySql(connectionString, serverVersion, mysqlOptions =>
                {
                    //NOTE: Tắt retry tự động tránh xung đột với UnitOfWork custom
                    mysqlOptions.EnableRetryOnFailure(0);
                });
            });
            #endregion

            #region File Upload Limits
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = long.MaxValue;
            });
            #endregion

            #region Redis
            var redisConfig = new ConfigurationOptions
            {
                EndPoints = { config["Redis:EndPoints"]! },
                User = config["Redis:User"] ?? "default",
                Password = config["Redis:Password"] ?? "",
            };

            var redis = ConnectionMultiplexer.Connect(redisConfig);
            services.AddSingleton<IConnectionMultiplexer>(redis);
            #endregion

            #region SeriLog
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
            #endregion

            #region Unit of Work 
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            return services;
        }
    }

}
