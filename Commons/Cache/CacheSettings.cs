using ManageLife.Base;

namespace ManageLife.Commons
{
    public static class CacheSettings
    {
        private const string _prefix = "manage_life:";

        public static CacheItem Permissions(string userId)
            => new($"{_prefix}permissions:{userId}");

        public static CacheItem Translations(string languageCode)
            => new($"{_prefix}translations:{languageCode}");

        public static CacheItem Languages()
            => new($"{_prefix}languages");

        public static CacheItem MenuItems()
            => new($"{_prefix}menu_items");
    }
}
