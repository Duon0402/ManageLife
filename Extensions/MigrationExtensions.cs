using ManageLife.Data;
using ManageLife.Middleware;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class MigrationExtensions
    {
        // Set "AutoMigrate": true in appsettings to auto-apply on startup (recommended for dev).
        // Leave false (default) for production — apply via admin UI to keep control.
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            var dbState = app.Services.GetRequiredService<DatabaseState>();

            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count == 0)
            {
                logger.LogInformation("Database is up to date.");
                return;
            }

            bool autoMigrate = config.GetValue<bool>("AutoMigrate");
            if (autoMigrate)
            {
                logger.LogInformation("AutoMigrate=true — applying {count} migration(s): {names}",
                    pending.Count, string.Join(", ", pending));
                await db.Database.MigrateAsync();
                logger.LogInformation("All migrations applied successfully.");
            }
            else
            {
                dbState.SetPending(pending);
                logger.LogWarning("{count} pending migration(s): {names}. Set AutoMigrate=true or use the admin UI to apply.",
                    pending.Count, string.Join(", ", pending));
            }
        }

        /// <summary>
        /// Apply all pending migrations — gọi từ admin endpoint.
        /// </summary>
        public static async Task<(bool success, string message, IReadOnlyList<string> applied)> RunMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbState = app.Services.GetRequiredService<DatabaseState>();

            try
            {
                var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Count == 0)
                {
                    dbState.ClearPending();
                    return (true, "Database đã được cập nhật, không có migration nào cần chạy.", []);
                }

                await db.Database.MigrateAsync();
                dbState.ClearPending();
                return (true, $"Đã apply {pending.Count} migration(s) thành công.", pending);
            }
            catch (Exception ex)
            {
                return (false, $"Migration thất bại: {ex.Message}", []);
            }
        }
    }
}
