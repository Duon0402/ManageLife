using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Middleware;
using ManageLife.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Controllers.Admin
{
    public class DatabaseController : WebAdminControllerBase
    {
        private readonly AppDbContext _db;
        private readonly DatabaseState _dbState;

        public DatabaseController(AppDbContext db, DatabaseState dbState)
        {
            _db = db;
            _dbState = dbState;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            var vm = new DatabaseViewModel
            {
                Applied = (await _db.Database.GetAppliedMigrationsAsync()).Reverse().ToList(),
                Pending = (await _db.Database.GetPendingMigrationsAsync()).ToList()
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

        [HttpGet]
        [ViewPermission]
        public async Task<IActionResult> GetStatus()
        {
            var applied = (await _db.Database.GetAppliedMigrationsAsync()).ToList();
            var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
            return Ok(new { applied, pending, isUpToDate = pending.Count == 0 });
        }
    }
}
