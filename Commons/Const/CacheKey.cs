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

        public static CacheKeyItem Languages(TimeSpan? expiry = null)
            => new($"{_prefix}languages", expiry);

        public static CacheKeyItem MenuItems()
            => new($"{_prefix}menu_items", TimeSpan.FromDays(7));
    }
}
