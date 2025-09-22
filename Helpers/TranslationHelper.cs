using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public static class TranslationHelper
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static ICacheService? _cache;

        public static void Configure(IServiceProvider services, ICacheService cacheService)
        {
            _httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
            _cache = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public static Task<string> TAsync(string key, params object[] args)
        {
            return TAsync(key, null, args);
        }

        public static async Task<string> TAsync(string key, string? langCode, params object[] args)
        {
            if (_httpContextAccessor?.HttpContext == null)
                return key;

            var httpContext = _httpContextAccessor.HttpContext;
            var translationService = httpContext.RequestServices.GetService<ITranslationService>();
            if (translationService == null)
                return key;

            if (langCode.IsEmpty())
                langCode = LanguageHelper.GetLanguage();

            var cacheKeyItem = CacheKey.Translations(langCode);

            if (!httpContext.Items.TryGetValue(cacheKeyItem.Key, out var dictObj))
            {
                Dictionary<string, string>? dict = null;

                if (_cache != null)
                {
                    dict = await _cache.TryGetValueAsync<Dictionary<string, string>>(cacheKeyItem.Key);
                }

                if (dict.IsEmpty())
                {
                    var req = new GetDictionaryTranslationByLanguageCodeRequest
                    {
                        LanguageCode = langCode,
                    };

                    var result = await translationService.GetDictionaryTranslationByLanguageCode(req);

                    if (result.IsOk() && result.Data.IsNotEmpty())
                    {
                        dict = result.Data;

                        if (_cache != null)
                            await _cache.SetAsync(cacheKeyItem.Key, dict, cacheKeyItem.Expiry);
                    }
                }

                dictObj = dict ?? new Dictionary<string, string>();

                httpContext.Items[cacheKeyItem.Key] = dictObj;
            }

            var translations = dictObj as Dictionary<string, string>;
            if (translations == null)
                return key;

            if (translations.TryGetValue(key, out var value))
            {
                return args != null && args.Length > 0
                    ? string.Format(value, args)
                    : value;
            }

            return key;
        }
    }
}
