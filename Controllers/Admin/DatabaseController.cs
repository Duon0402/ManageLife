using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Middleware;
using ManageLife.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManageLife.Controllers.Admin
{
    public class DatabaseController : WebAdminControllerBase
    {
        private readonly AppDbContext _db;
        private readonly DatabaseState _dbState;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public DatabaseController(AppDbContext db, DatabaseState dbState, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _dbState = dbState;
            _config = config;
            _env = env;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            var vm = new DatabaseViewModel
            {
                Applied = (await _db.Database.GetAppliedMigrationsAsync()).Reverse().ToList(),
                Pending = (await _db.Database.GetPendingMigrationsAsync()).ToList(),
                HasMigrationKey = !string.IsNullOrEmpty(_config["MigrationKeyHash"])
            };
            return View(vm);
        }

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Migrate(CancellationToken ct)
        {
            try
            {
                var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Count == 0)
                    return Result.Ok("Database đã up-to-date.");

                await _db.Database.MigrateAsync(ct);
                _dbState.ClearPending();
                return Result.Ok($"Đã apply {pending.Count} migration(s) thành công.");
            }
            catch (Exception ex)
            {
                return Result.Exception("Migration thất bại", ex);
            }
        }

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> SetMigrationKey([FromBody] SetMigrationKeyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
                return Result.Error("01", "Key không được để trống.");

            var hash = PendingMigrationMiddleware.HashKey(request.Key);
            await WriteConfigValueAsync("MigrationKeyHash", hash);
            (_config as IConfigurationRoot)?.Reload();

            return Result.Ok("Migration key đã được cập nhật.");
        }

        [HttpGet]
        [ViewPermission]
        public async Task<IActionResult> GetStatus()
        {
            var applied = (await _db.Database.GetAppliedMigrationsAsync()).ToList();
            var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
            return Ok(new { applied, pending, isUpToDate = pending.Count == 0 });
        }

        private async Task WriteConfigValueAsync(string key, string value)
        {
            var path = Path.Combine(_env.ContentRootPath, "appsettings.json");
            var json = await System.IO.File.ReadAllTextAsync(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
            dict[key] = JsonSerializer.SerializeToElement(value);
            var updated = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(path, updated);
        }
    }

    public class SetMigrationKeyRequest
    {
        public string Key { get; set; } = default!;
    }
}
