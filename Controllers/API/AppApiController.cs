using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/app")]
    public class AppApiController : ApiControllerBase
    {
        private readonly IAppConfigService _appConfigService;

        public AppApiController(IAppConfigService appConfigService)
        {
            _appConfigService = appConfigService;
        }

        [HttpGet("config")]
        public async Task<Result<AppConfigModel>> GetConfig(CancellationToken ct)
        {
            return await _appConfigService.GetConfigAsync(ct);
        }
    }
}
