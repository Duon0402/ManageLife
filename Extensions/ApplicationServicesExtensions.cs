using AutoMapper;
using ManageLife.ApiClients;
using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Services;
using ManageLife.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Text;
using Telegram.Bot;

namespace ManageLife.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Set EPPlus license 1 lần
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            services.AddApplicationSettings(config);
            services.AddApplicationCustomServices();
            services.AddRepositories();

            // DataProtection: lưu key ra file để key sống qua app restart (tránh user bị logout)
            var dpKeysPath = Path.Combine(AppContext.BaseDirectory, "dataprotection-keys");
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
                .SetApplicationName("ManageLife");
            services.AddHttpContextAccessor();
            services.AddScoped<IMenuRegister, MenuRegister>();
            services.AddHttpClient();

            services.AddSingleton<TelegramBotClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;
                return new TelegramBotClient(settings.BotToken
                    ?? throw new InvalidOperationException("TelegramSettings:BotToken is not configured."));
            });

            services.AddHttpClient<CronJobApiClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CronJobSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    settings.ApiKey ?? throw new InvalidOperationException("CronJob:ApiKey is not configured."));
            });

            #region JWT Authentication
            var jwt = config.GetSection(JwtSettings.Section).Get<JwtSettings>()!;
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
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();
            #endregion

            #region AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());
            #endregion

            #region AddDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = config.GetConnectionString("DefaultConnection") ?? "";
                var versionStr = config["MySqlVersion"] ?? "8.0.0";
                var serverVersion = new MySqlServerVersion(Version.Parse(versionStr));
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
            var redisSettings = config.GetSection(RedisSettings.Section).Get<RedisSettings>()!;
            var redisConfig = new ConfigurationOptions
            {
                EndPoints = { redisSettings.EndPoints },
                User = redisSettings.User,
                Password = redisSettings.Password,
            };

            var redis = ConnectionMultiplexer.Connect(redisConfig);
            services.AddSingleton<IConnectionMultiplexer>(redis);
            #endregion

            #region SeriLog
            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
            #endregion

            #region Unit of Work 
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            return services;
        }

        public static WebApplication UseMapperBase(this WebApplication app)
        {
            MapperBase.Configure(app.Services.GetRequiredService<IMapper>());
            return app;
        }
    }

}
