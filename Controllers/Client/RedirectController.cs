using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    [AllowAnonymous]
    public class RedirectController : Controller
    {
        private readonly IShortUrlService _service;

        public RedirectController(IShortUrlService service)
        {
            _service = service;
        }

        [HttpGet("r/{code}")]
        public async Task<IActionResult> Index(string code, CancellationToken ct)
        {
            var result = await _service.GetByCodeAsync(new GetShortUrlByCodeRequest { Code = code }, ct);

            if (!result.IsOk())
                return NotFound();

            var shortUrl = result.Data;

            if (shortUrl.ExpireAt.HasValue && shortUrl.ExpireAt.Value < DateTimeHelper.UtcNow())
                return NotFound();

            await _service.RecordClickAsync(new RecordShortUrlClickRequest
            {
                Code = code,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                Referer = Request.Headers.Referer.ToString()
            }, ct);

            return Redirect(shortUrl.OriginalUrl);
        }
    }
}
