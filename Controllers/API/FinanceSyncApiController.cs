using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/finance")]
    public class FinanceSyncApiController : ApiControllerBase
    {
        private readonly IFinanceSyncService _syncService;

        public FinanceSyncApiController(IFinanceSyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>App gửi các thao tác làm trên máy (đúng thứ tự), nhận lại thay đổi từ mốc `since`.</summary>
        [HttpPost("sync")]
        public async Task<Result<FinanceSyncModel>> Sync([FromBody] FinanceSyncRequest request, CancellationToken ct)
        {
            return await _syncService.SyncAsync(request, ct);
        }
    }
}
