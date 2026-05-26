using ManageLife.Core;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class LanguageController : WebClientControllerBase
    {
        private readonly ILanguageService _service;
        private readonly ITranslationService _translationService;
        private readonly ILanguageContext _languageContext;

        public LanguageController(ILanguageService service, ITranslationService translationService, ILanguageContext languageContext)
        {
            _service = service;
            _translationService = translationService;
            _languageContext = languageContext;
        }

        [HttpPost]
        public async Task<Result<ChangeLanguageResult>> ChangeLanguage([FromBody] ChangeLanguageRequest request, CancellationToken ct)
        {
            var currentLanguage = _languageContext.GetCurrentLanguage();
            var rs = await _service.ChangeLanguageAsync(request, currentLanguage, ct);

            if (rs.IsOk())
            {
                _languageContext.SetCurrentLanguage(rs.Data.LanguageCode);
            }

            return rs;
        }

        [HttpGet]
        public async Task<Result<List<LanguageModel>>> GetListLanguages(CancellationToken ct)
        {
            return await _service.GetListLanguagesAsync(ct);
        }

        [HttpPost]
        public async Task<Result<LanguageModel>> GetLanguageByCode([FromBody] GetLanguageByCodeRequest request, CancellationToken ct)
        {
            return await _service.GetLanguageByCodeAsync(request, ct);
        }

        [HttpGet]
        public async Task<Result<Dictionary<string, string>>> GetTranslations(CancellationToken ct)
        {
            var languageCode = _languageContext.GetCurrentLanguage();
            return await _translationService.GetDictionaryTranslationByLanguageCode(
                new GetDictionaryTranslationByLanguageCodeRequest { LanguageCode = languageCode }, ct);
        }
    }
}
