using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class VocabController : WebClientControllerBase
    {
        private readonly IVocabWordService _wordService;
        private readonly IVocabTopicService _topicService;
        private readonly IVocabDeckService _deckService;
        private readonly IVocabStudyService _studyService;
        private readonly ISettingContext _settingContext;

        public VocabController(
            IVocabWordService wordService,
            IVocabTopicService topicService,
            IVocabDeckService deckService,
            IVocabStudyService studyService,
            ISettingContext settingContext)
        {
            _wordService = wordService;
            _topicService = topicService;
            _deckService = deckService;
            _studyService = studyService;
            _settingContext = settingContext;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableVocab, true))
                return NotFound();
            return View();
        }

        [AccessPagePermission]
        public async Task<IActionResult> Deck(string id, CancellationToken ct)
        {
            var result = await _deckService.GetByIdAsync(id, ct);
            if (!result.IsOk()) return RedirectToAction("Index");
            ViewBag.DeckId = result.Data!.Id;
            ViewBag.DeckName = result.Data!.Name;
            return View("Deck");
        }

        [AccessPagePermission]
        public async Task<IActionResult> Study(string id, CancellationToken ct)
        {
            var result = await _deckService.GetByIdAsync(id, ct);
            if (!result.IsOk()) return RedirectToAction("Index");
            ViewBag.DeckId = result.Data!.Id;
            ViewBag.DeckName = result.Data!.Name;
            return View("Study");
        }

        // ── Topic ─────────────────────────────────────────────────────────────

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<VocabTopicModel>>> GetTopics(CancellationToken ct)
            => await _topicService.GetListAsync(ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateTopic([FromBody] CreateVocabTopicRequest request, CancellationToken ct)
            => await _topicService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPut]
        public async Task<Result> UpdateTopic([FromBody] UpdateVocabTopicRequest request, CancellationToken ct)
            => await _topicService.UpdateAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> DeleteTopic(string id, CancellationToken ct)
            => await _topicService.DeleteAsync(id, ct);

        // ── Deck ──────────────────────────────────────────────────────────────

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<VocabDeckModel>>> GetDecks(string? topicId, CancellationToken ct)
            => await _deckService.GetListAsync(topicId, ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateDeck([FromBody] CreateVocabDeckRequest request, CancellationToken ct)
            => await _deckService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPut]
        public async Task<Result> UpdateDeck([FromBody] UpdateVocabDeckRequest request, CancellationToken ct)
            => await _deckService.UpdateAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> DeleteDeck(string id, CancellationToken ct)
            => await _deckService.DeleteAsync(id, ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> AddWordToDeck([FromBody] AddWordToDeckRequest request, CancellationToken ct)
            => await _deckService.AddWordAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> RemoveWordFromDeck(string deckId, string wordId, CancellationToken ct)
            => await _deckService.RemoveWordAsync(deckId, wordId, ct);

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<VocabWordModel>>> GetDeckWords(string deckId, CancellationToken ct)
            => await _deckService.GetWordsAsync(deckId, ct);

        // ── Study ─────────────────────────────────────────────────────────────

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<StudyCardModel>>> GetDueCards(string deckId, CancellationToken ct)
            => await _studyService.GetDueCardsAsync(deckId, ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> SubmitReview([FromBody] SubmitReviewRequest request, CancellationToken ct)
            => await _studyService.SubmitReviewAsync(request, ct);

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
