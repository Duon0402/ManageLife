using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers
{
    public class LanguageController : WebControllerBase
    {
        private readonly ILanguageService _service;

        public LanguageController(AppDbContext context, ILanguageService service, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
        }

        [HttpPost]
        public Task<Result<string?>> ChangeLanguage([FromBody] ChangeLanguageRequest request)
        {
            var rs = _service.ChangeLanguageAsync(request);
            return rs;
        }

        [HttpGet]
        public async Task<Result<List<LanguageModel>>> GetListLanguages()
        {
            var rs = await _service.GetListLanguagesAsync();
            return rs;
        }
    }
}
