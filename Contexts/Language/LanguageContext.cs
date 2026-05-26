using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Contexts
{
    public class LanguageContext : ILanguageContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;

        public LanguageContext(IHttpContextAccessor httpContextAccessor, ILanguageService languageService)
        {
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
        }

        private HttpContext HttpContext =>
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        public string GetCurrentLanguage()
        {
            if (HttpContext.Request.Cookies.TryGetValue(LanguageConst.COOKIE_NAME, out var lang)
                && lang.IsNotEmpty())
            {
                return lang;
            }

            SetCurrentLanguage(LanguageConst.DEFAULT_LANGUAGE);
            return LanguageConst.DEFAULT_LANGUAGE;
        }

        public async Task<string> GetCurrentLanguageNameAsync(string? languageCode = null)
        {
            languageCode ??= GetCurrentLanguage();

            var rq = new GetLanguageByCodeRequest { LanguageCode = languageCode };
            var rs = await _languageService.GetLanguageByCodeAsync(rq);
            if (rs.IsOk() && rs.Data != null)
            {
                return rs.Data.Name;
            }

            return languageCode;
        }

        public void SetCurrentLanguage(string lang)
        {
            HttpContext.Response.Cookies.Append(LanguageConst.COOKIE_NAME, lang, new CookieOptions
            {
                Expires = DateTimeHelper.UtcNow().AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Secure = HttpContext.Request.IsHttps
            });
        }
    }
}
