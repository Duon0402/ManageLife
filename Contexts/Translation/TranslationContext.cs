using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Contexts
{
    public class TranslationContext : ITranslationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITranslationService _translationService;
        private readonly ICacheService _cacheService;
        private readonly ILanguageContext _languageContext;

        public TranslationContext(
            IHttpContextAccessor httpContextAccessor,
            ITranslationService translationService,
            ICacheService cacheService,
            ILanguageContext languageContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _translationService = translationService;
            _cacheService = cacheService;
            _languageContext = languageContext;
        }

        private HttpContext HttpContext =>
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        public async Task<string> TranslateAsync(string key, params object[] args)
        {
            return await TranslateAsync(key, null, args);
        }

        public async Task<string> TranslateAsync(string key, string? languageCode, params object[] args)
        {
            languageCode ??= _languageContext.GetCurrentLanguage();

            var req = new GetDictionaryTranslationByLanguageCodeRequest { LanguageCode = languageCode };
            var res = await _translationService.GetDictionaryTranslationByLanguageCode(req);

            if (res.IsOk() && res.Data != null && res.Data.TryGetValue(key, out var value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }

            return key;
        }
    }
}
