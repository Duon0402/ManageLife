using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class ShortUrlController : WebClientControllerBase
    {
        private readonly IShortUrlService _service;

        public ShortUrlController(IShortUrlService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index() => View();

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
