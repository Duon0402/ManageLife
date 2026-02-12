using AutoMapper;
using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Middlewares;
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

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Cấu hình Kestrel (request body limit)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

var app = builder.Build();

// ====== Serilog request logging ======
app.UseSerilogRequestLogging();
// =====================================

GlobalHttpContext.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>()
);

// ======= SEED DATA =========
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await Seed.SeedData(context);

    // ======= REGISTER PERMISSIONS =========
    var services = scope.ServiceProvider;
    await services.RegisterPermissionsAsync(typeof(Program).Assembly);
    // ======================================
}
// ===========================

// Configure MapperBase
var mapper = app.Services.GetRequiredService<IMapper>();
MapperBase.Configure(mapper);

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

app.Run();
