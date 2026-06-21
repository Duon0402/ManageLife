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

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<TranslationModel>>> GetList([FromQuery] GetListTranslationsRequest request, CancellationToken ct)
        {
            return await _service.GetListTranslationsAsync(request);
        }

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateTranslationRequest request)
        {
            return await _service.CreateTranslationAsync(request);
        }

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateTranslationRequest request)
        {
            return await _service.UpdateTranslationAsync(request);
        }

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] DeleteTranslationRequest request)
        {
            return await _service.DeleteTranslationAsync(request);
        }

        [HttpGet]
        [ViewPermission]
        public async Task<IActionResult> DownloadTemplate(CancellationToken ct)
        {
            var rs = await _service.DownloadTranslationTemplateExcelAsync(ct);
            if (!rs.IsOk())
                return BadRequest(rs.Message);

            return File(rs.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "translation_template.xlsx");
        }

        [HttpPost]
        [InsertPermission]
        public async Task<Result> ImportTemplate([FromForm] ImportTranslationExcelRequest request, CancellationToken ct)
        {
            return await _service.ImportTranslationExcelAsync(request, ct);
        }
    }
}
