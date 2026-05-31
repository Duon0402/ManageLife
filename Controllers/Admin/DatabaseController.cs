using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Controllers.Admin
{
    public class DatabaseController : WebAdminControllerBase
    {
        private readonly AppDbContext _db;

        public DatabaseController(AppDbContext db)
        {
            _db = db;
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
        public async Task<IActionResult> Migrate(CancellationToken ct)
        {
            try
            {
                var pending = (await _db.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Count == 0)
                    return Ok(new { success = true, message = "Database đã up-to-date.", applied = Array.Empty<string>() });

                await _db.Database.MigrateAsync(ct);
                return Ok(new { success = true, message = $"Đã apply {pending.Count} migration(s).", applied = pending });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
