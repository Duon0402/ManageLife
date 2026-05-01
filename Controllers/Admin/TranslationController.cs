using ManageLife.Core;
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

        public TranslationController(ITranslationService service, ILanguageService languageService)
        {
            _service = service;
            _languageService = languageService;
        }

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var viewModel = new TranslationViewModel();

            var rs = await _languageService.GetListLanguagesAsync(ct);
            if (rs.IsOk() && rs.Data.IsNotEmpty())
            {
                viewModel.Languages = rs.Data.Select(x => new KeyValueModel(x.Id, x.Name)).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<Result<List<TranslationModel>>> GetListTranslations([FromBody] GetListTranslationsRequest request, CancellationToken ct)
        {
            return await _service.GetListTranslationsAsync(request, ct);
        }

        [HttpPost]
        public async Task<Result> CreateTranslation([FromBody] CreateTranslationRequest request, CancellationToken ct)
        {
            return await _service.CreateTranslationAsync(request, ct);
        }
    }
}
