using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class TranslationController : WebAdminControllerBase
    {
        private readonly ITranslationService _service;

        private readonly ILanguageService _languageService;

        public TranslationController(AppDbContext context, ITranslationService service, ILanguageService languageService, ILogger? logger = null) : base(context, logger)
        {
            _service = service;
            _languageService = languageService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new TranslationViewModel();

            var rs = await _languageService.GetListLanguagesAsync();
            if (rs.IsOk() && rs.Data.IsNotEmpty())
            {
                viewModel.Languages = rs.Data.Select(x => new KeyValueModel(x.Id, x.Name)).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<Result<List<TranslationModel>>> GetListTranslations([FromBody] GetListTranslationsRequest request)
        {
            var rs = await _service.GetListTranslationsAsync(request);
            return rs;
        }

        [HttpPost]
        public async Task<Result> CreateTranslation([FromBody] CreateTranslationRequest request)
        {
            var rs = await _service.CreateTranslationAsync(request);
            return rs;
        }
    }
}
