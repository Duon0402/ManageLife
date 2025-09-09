using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class LanguageController : WebAdminControllerBase
    {
        private readonly ILanguageService _service;

        public LanguageController(AppDbContext context, ILanguageService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<Result<List<LanguageModel>>> GetListLanguages()
        {
            var rs = await _service.GetListLanguagesAsync();
            return rs;
        }

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateLanguage([FromBody] CreateLanguageRequest request)
        {
            var rs = await _service.CreateLanguageAsync(request);
            return rs;
        }
    }
}
