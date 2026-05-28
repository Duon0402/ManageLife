using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.Hubs;
using ManageLife.Middleware;
using ManageLife.Services;
using Serilog;

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

// Cấu hình Kestrel (request body limit)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

builder.Services.AddHostedService<TelegramUploadWorker>();

var app = builder.Build();

await app.ApplyMigrationsAsync();

// ====== Serilog request logging ======
app.UseSerilogRequestLogging();
// =====================================

app.UseMapperBase();

// ======= SEED DATA =========
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await Seed.SeedData(context, config);

    // ======= REGISTER PERMISSIONS =========
    var services = scope.ServiceProvider;
    await services.RegisterPermissionsAsync(typeof(Program).Assembly);
    // ======================================
}
// ===========================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin_default",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<ChatHub>("/chathub");

app.Run();
