using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class VocabController : WebClientControllerBase
    {
        private readonly IVocabWordService _wordService;

        public VocabController(IVocabWordService wordService)
        {
            _wordService = wordService;
        }

        [AccessPagePermission]
        public IActionResult Index() => View();

        // ── Word ──────────────────────────────────────────────────────────────

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<VocabWordModel>>> GetWords([FromQuery] GetListVocabWordsRequest request, CancellationToken ct)
            => await _wordService.GetListAsync(request, ct);

        [ViewPermission]
        [HttpGet]
        public async Task<Result<DictionaryLookupResult>> Lookup([FromQuery] LookupWordRequest request, CancellationToken ct)
            => await _wordService.LookupFromDictionaryAsync(request, ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateWord([FromBody] CreateVocabWordRequest request, CancellationToken ct)
            => await _wordService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPut]
        public async Task<Result> UpdateWord([FromBody] UpdateVocabWordRequest request, CancellationToken ct)
            => await _wordService.UpdateAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> DeleteWord(string id, CancellationToken ct)
            => await _wordService.DeleteAsync(new DeleteVocabWordRequest { Id = id }, ct);
    }
}
