using ManageLife.Data;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            if (!app.Environment.IsProduction())
                return;

            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();
        }
    }
}
