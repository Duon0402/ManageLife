using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class ShortUrlController : WebClientControllerBase
    {
        private readonly IShortUrlService _service;
        private readonly ISettingContext _settingContext;

        public ShortUrlController(IShortUrlService service, ISettingContext settingContext)
        {
            _service = service;
            _settingContext = settingContext;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableShortUrl, true))
                return NotFound();
            return View();
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<ShortUrlModel>>> GetList(CancellationToken ct)
            => await _service.GetListAsync(ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateShortUrlRequest request, CancellationToken ct)
            => await _service.CreateAsync(request, ct);

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] DeleteShortUrlRequest request, CancellationToken ct)
            => await _service.DeleteAsync(request, ct);
    }
}
