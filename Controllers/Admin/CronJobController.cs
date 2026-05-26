using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class CronJobController : WebAdminControllerBase
    {
        private readonly ICronJobService _service;

        public CronJobController(ICronJobService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result> GetListCronJobs(CancellationToken ct)
        {
            return await _service.GetListCronJobsAsync(ct);
        }
    }
}
