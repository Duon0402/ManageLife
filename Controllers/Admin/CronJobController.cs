using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
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
        public async Task<Result<List<CronJobViewModel>>> GetList(CancellationToken ct)
        {
            return await _service.GetListAsync(ct);
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<CronJobViewModel>> GetById(int jobId, CancellationToken ct)
        {
            return await _service.GetByIdAsync(jobId, ct);
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<CronJobHistoryModel>>> GetHistory(int jobId, CancellationToken ct)
        {
            return await _service.GetHistoryAsync(jobId, ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create([FromBody] SaveCronJobRequest request, CancellationToken ct)
        {
            return await _service.CreateAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> Update([FromBody] SaveCronJobRequest request, CancellationToken ct)
        {
            return await _service.UpdateAsync(request, ct);
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> Delete([FromBody] CronJobIdRequest request, CancellationToken ct)
        {
            return await _service.DeleteAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result<string>> SyncSystemJobs(CancellationToken ct)
        {
            return await _service.SyncSystemJobsAsync(ct);
        }

    }
}
