using ManageLife.Interfaces;

namespace ManageLife.Helpers
{
    public static class LanguageHelper
    {
        private const string _cookieName = "language";
        private const string _defaultLanguage = "vi-VN";

        private static IHttpContextAccessor _httpContextAccessor = default!;
        private static IServiceProvider _serviceProvider = default!;

        public static void Configure(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private static HttpContext Current =>
            _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");

        public static string GetLanguage()
        {
            if (Current.Request.Cookies.TryGetValue(_cookieName, out var lang) && !string.IsNullOrEmpty(lang))
                return lang;

            SetLanguage(_defaultLanguage);
            return _defaultLanguage;
        }

        public static void SetLanguage(string lang)
        {
            Current.Response.Cookies.Append(_cookieName, lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Secure = Current.Request.IsHttps
            });
        }

        public static async Task<string> GetLanguageName(string? languageCode = null)
        {
            languageCode ??= GetLanguage();

            using var scope = _serviceProvider.CreateScope();
            var languageService = scope.ServiceProvider.GetRequiredService<ILanguageService>();

            var rs = await languageService.GetListLanguagesAsync();
            if (rs.IsOk() && rs.Data != null)
            {
                var found = rs.Data.FirstOrDefault(x => x.Code == languageCode);
                if (found != null)
                    return found.Name ?? languageCode;
            }

            return languageCode;
        }
    }
}
