using ManageLife.Base;

namespace ManageLife.Commons
{
    public static class CacheKey
    {

        private const string _prefix = "manage_life:";

        public static CacheKeyItem Permissions(string userId, TimeSpan? expiry = null)
            => new($"{_prefix}permissions:{userId}", expiry);

        public static CacheKeyItem Translations(string languageCode, TimeSpan? expiry = null)
            => new($"{_prefix}translations:{languageCode}", expiry);
    }
}
