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
builder.Services.AddControllersWithViews(options =>
{
    //NOTE: Production không trả stack trace (ErrorContent) ra client, lỗi đã được log ở service
    if (!builder.Environment.IsDevelopment())
        options.Filters.Add<ManageLife.Core.HideErrorContentFilter>();
});
builder.Services.AddSignalR();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<DatabaseState>();

builder.Services.AddRateLimiter(options =>
{
    // Giới hạn theo IP để một người gõ sai không chặn đăng nhập của mọi người khác.
    // Host đằng sau IIS/ANCM in-process nên RemoteIpAddress là IP client thật; điểm còn hở chung
    // là mạng CGNAT/office (nhiều người dùng chung 1 IP public) — đã tách bucket riêng cho
    // luồng quên mật khẩu để nhóm đó không ăn quota của nhau.
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }));
    // Luồng quên mật khẩu (gửi mã → kiểm mã → đặt lại): tách khỏi "login" vì 1 lượt cần 3 request
    // liền + người dùng hay gõ sai; chia chung thì người sau CGNAT sẽ 429 oan.
    options.AddPolicy("password-reset", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }));
    // Tạo mã liên kết Telegram: mỗi mã giữ 5 phút trong Redis, chặn spam tạo mã (theo IP vì chạy trước xác thực)
    options.AddPolicy("telegram-link", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }));
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
    //NOTE: Lỗi chưa bắt (service đã tự bắt phần lớn): /api trả JSON Result mã "99" như mọi API để app đọc được,
    // không trả trang lỗi/body rỗng; không lộ chi tiết lỗi ra ngoài môi trường dev (đã ghi log ở middleware)
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await context.Response.WriteAsJsonAsync(new ManageLife.Core.Result("99", "Đã có lỗi xảy ra, vui lòng thử lại sau"));
            return;
        }
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Đã có lỗi xảy ra, vui lòng thử lại sau");
    }));
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
