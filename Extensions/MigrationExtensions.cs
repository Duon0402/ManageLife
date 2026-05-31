using ManageLife.Data;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class MigrationExtensions
    {
        /// <summary>
        /// Dev: tự động apply migrations khi startup.
        /// Production: chỉ log pending migrations, không tự apply (dùng endpoint admin).
        /// </summary>
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            if (app.Environment.IsDevelopment())
            {
                try
                {
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully (Development).");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Migration failed in Development. App will continue.");
                }
            }
            else
            {
                var pending = await db.Database.GetPendingMigrationsAsync();
                var list = pending.ToList();
                if (list.Count > 0)
                {
                    logger.LogWarning("Production: {count} pending migration(s): {names}. Use POST /Admin/Database/Migrate to apply.",
                        list.Count, string.Join(", ", list));
                }
                else
                {
                    logger.LogInformation("Database is up to date.");
                }
            }
        }

        /// <summary>
        /// Apply all pending migrations — gọi từ admin endpoint.
        /// </summary>
        public static async Task<(bool success, string message, IReadOnlyList<string> applied)> RunMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Count == 0)
                    return (true, "Database đã được cập nhật, không có migration nào cần chạy.", []);

                await db.Database.MigrateAsync();
                return (true, $"Đã apply {pending.Count} migration(s) thành công.", pending);
            }
            catch (Exception ex)
            {
                return (false, $"Migration thất bại: {ex.Message}", []);
            }
        }
    }
}
