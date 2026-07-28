using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class AnkiCardController : WebClientControllerBase
    {
        private readonly IAnkiCardService _cardService;
        private readonly ISettingContext _settingContext;

        public AnkiCardController(IAnkiCardService cardService, ISettingContext settingContext)
        {
            _cardService = cardService;
            _settingContext = settingContext;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableAnkiCard, true))
                return NotFound();
            return View();
        }

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<AnkiCardModel>>> GetList(CancellationToken ct)
            => await _cardService.GetListAsync(ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> Create([FromBody] CreateAnkiCardRequest request, CancellationToken ct)
            => await _cardService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPut]
        public async Task<Result> Update([FromBody] UpdateAnkiCardRequest request, CancellationToken ct)
            => await _cardService.UpdateAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> Delete(string id, CancellationToken ct)
            => await _cardService.DeleteAsync(id, ct);

        [ViewPermission]
        [HttpGet]
        public async Task<IActionResult> ExportAnki(CancellationToken ct)
        {
            var result = await _cardService.GetAllForExportAsync(ct);
            if (!result.IsOk() || result.Data == null || !result.Data.Any())
                return BadRequest("Chưa có thẻ nào để xuất.");

            var bytes = AnkiPackageBuilder.Build(result.Data);
            return File(bytes, "application/octet-stream", $"anki-cards-{DateTime.UtcNow:yyyyMMdd}.apkg");
        }

        [ViewPermission]
        [HttpGet]
        public async Task<IActionResult> ExportAnkiText(CancellationToken ct)
        {
            var result = await _cardService.GetAllForExportAsync(ct);
            if (!result.IsOk() || result.Data == null || !result.Data.Any())
                return BadRequest("Chưa có thẻ nào để xuất.");

            var bytes = AnkiTextExporter.Build(result.Data);
            return File(bytes, "text/plain", $"anki-cards-{DateTime.UtcNow:yyyyMMdd}.txt");
        }
    }
}
