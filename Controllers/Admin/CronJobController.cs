using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class CronJobController : WebAdminControllerBase
    {
        private readonly ICronJobService _service;

        public CronJobController(AppDbContext context, ICronJobService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        public override IActionResult Index()
        {
            return base.Index();
        }

        public async Task<Result> GetListCronJobs()
        {
            var rs = await _service.GetListCronJobsAsync();
            return rs;
        }

    }
}
