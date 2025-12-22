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

        public static CacheKeyItem MenuItems(TimeSpan? expiry = null)
            => new($"{_prefix}menu_items", expiry);

        public static CacheKeyItem SecurityStamp(string userId, TimeSpan? expiry = null)
            => new($"{_prefix}security_stamp:{userId}", expiry);
    }
}
