using ManageLife.Base;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class LanguageController : WebClientControllerBase
    {
        private readonly ILanguageService _service;
        private readonly ILanguageContext _languageContext;
        private readonly ITranslationService _translationService;

        public LanguageController(ILanguageService service, ILanguageContext languageContext, ITranslationService translationService)
        {
            _service = service;
            _languageContext = languageContext;
            _translationService = translationService;
        }

        [HttpPost]
        public async Task<Result<ChangeLanguageResult>> ChangeLanguage([FromBody] ChangeLanguageRequest request)
        {
            var currrentLangugage = _languageContext.GetCurrentLanguage();
            var rs = await _service.ChangeLanguageAsync(request, currrentLangugage);

            if (rs.IsOk())
            {
                _languageContext.SetCurrentLanguage(rs.Data.LanguageCode);
            }

            return rs;
        }

        [HttpGet]
        public async Task<Result<List<LanguageModel>>> GetListLanguages()
        {
            var rs = await _service.GetListLanguagesAsync();
            return rs;
        }

        [HttpPost]
        public async Task<Result<LanguageModel>> GetLanguageByCode([FromBody] GetLanguageByCodeRequest request)
        {
            var rs = await _service.GetLanguageByCodeAsync(request);
            return rs;
        }

        [HttpGet]
        public async Task<Result<Dictionary<string, string>>> GetTranslations()
        {
            var languageCode = _languageContext.GetCurrentLanguage();
            var req = new GetDictionaryTranslationByLanguageCodeRequest { LanguageCode = languageCode };
            var rs = await _translationService.GetDictionaryTranslationByLanguageCode(req);
            return rs;
        }
    }
}
