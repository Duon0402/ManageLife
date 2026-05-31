using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class LanguageController : WebAdminControllerBase
    {
        private readonly ILanguageService _service;

        public LanguageController(ILanguageService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<LanguageModel>>> GetList(CancellationToken ct)
        {
            return await _service.GetListLanguagesAsync(ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create([FromBody] CreateLanguageRequest request, CancellationToken ct)
        {
            return await _service.CreateLanguageAsync(request, ct);
        }

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> Update([FromBody] UpdateLanguageRequest request, CancellationToken ct)
        {
            return await _service.UpdateLanguageAsync(request, ct);
        }

        [DeletePermission]
        [HttpPost]
        public async Task<Result> Delete([FromBody] DeleteLanguageRequest request, CancellationToken ct)
        {
            return await _service.DeleteLanguageAsync(request, ct);
        }
    }
}
