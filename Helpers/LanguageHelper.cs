namespace ManageLife.Helpers
{
    public static class LanguageHelper
    {
        private const string _cookieName = "language";
        private const string _defaultLanguage = "vi-VN";

        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private static HttpContext? Current => _httpContextAccessor?.HttpContext;

        public static string GetLanguage()
        {
            var httpContext = Current;
            if (httpContext == null)
                return _defaultLanguage;

            if (httpContext.Request.Cookies.TryGetValue(_cookieName, out var lang))
            {
                if (!string.IsNullOrEmpty(lang))
                    return lang;
            }

            SetLanguage(_defaultLanguage);
            return _defaultLanguage;
        }

        public static void SetLanguage(string lang)
        {
            var httpContext = Current;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Append(_cookieName, lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Secure = httpContext.Request.IsHttps
            });
        }
    }
}
