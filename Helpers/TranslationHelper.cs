using ManageLife.Interfaces;

namespace ManageLife.Helpers
{
    // TODO: Thêm helper để dịch đa ngôn ngữ
    public static class TranslationHelper
    {
        private static IServiceProvider? _services;

        public static void Configure(IServiceProvider services)
        {
            _services = services;
        }

        public static string T(string key, params object[] args)
        {
            return T(key, null, args);
        }

        public static string T(string key, string? langCode, params object[] args)
        {
            if (_services == null) return key;

            var httpContextAccessor = _services.GetService<IHttpContextAccessor>();
            var translationService = _services.GetService<ITranslationService>();
            if (translationService == null) return key;

            var lang = langCode;


            return key;
        }
    }
}
