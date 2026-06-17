using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.Hubs;
using ManageLife.Middleware;
using ManageLife.Services;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ================== SERILOG ==================
var logPath = Path.Combine(AppContext.BaseDirectory, "Logs");
if (!Directory.Exists(logPath))
{
    Directory.CreateDirectory(logPath);
}

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});
// =============================================

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<DatabaseState>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Giới hạn upload 200MB — tránh DoS qua large file upload
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200L * 1024 * 1024;
});

builder.Services.AddHostedService<TelegramUploadWorker>();

var app = builder.Build();

await app.ApplyMigrationsAsync();

// ====== Serilog request logging ======
app.UseSerilogRequestLogging();
// =====================================

app.UseMapperBase();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await Seed.SeedData(context, config);
    }

    await scope.ServiceProvider.RegisterPermissionsAsync(typeof(Program).Assembly);
    await scope.ServiceProvider.RegisterSettingsAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<PendingMigrationMiddleware>();
app.UseMiddleware<MaintenanceMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin_default",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "short_url_redirect",
    pattern: "r/{code}",
    defaults: new { controller = "Redirect", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ChatHub>("/chathub");

app.Run();
