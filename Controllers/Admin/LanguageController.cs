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
        public async Task<Result<List<LanguageModel>>> GetListLanguages(CancellationToken ct)
        {
            return await _service.GetListLanguagesAsync(ct);
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateLanguage([FromBody] CreateLanguageRequest request, CancellationToken ct)
        {
            return await _service.CreateLanguageAsync(request, ct);
        }
    }
}
