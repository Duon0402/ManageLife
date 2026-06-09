using ManageLife.Data;
using ManageLife.Middleware;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            var dbState = app.Services.GetRequiredService<DatabaseState>();

            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count > 0)
            {
                dbState.SetPending(pending);
                logger.LogWarning("{count} pending migration(s): {names}. Use POST /Admin/Database/Migrate to apply.",
                    pending.Count, string.Join(", ", pending));
            }
            else
            {
                logger.LogInformation("Database is up to date.");
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
