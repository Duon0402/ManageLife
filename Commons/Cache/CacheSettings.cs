using ManageLife.Core;

namespace ManageLife.Commons
{
    public static class CacheSettings
    {
        private const string _prefix = "manage_life:";

        public static CacheItem Permissions(string userId)
            => new($"{_prefix}permissions:{userId}");

        public static CacheItem Translations(string languageCode)
            => new($"{_prefix}translations:{languageCode}");

        // Memory cache: data nhỏ, đọc trên mọi request, Redis latency cao (~150ms)
        public static CacheItem Languages()
            => new($"{_prefix}languages", CacheMode.Memory);

        public static CacheItem MenuItems()
            => new($"{_prefix}menu_items", CacheMode.Memory);

        public static CacheItem SecurityStamp(string userId)
            => new($"{_prefix}security_stamp:{userId}", expiry: TimeSpan.FromDays(7));

        public static CacheItem RoleAssignedPermissions(string roleId)
            => new($"{_prefix}role_permissions:assigned:{roleId}");

        public static CacheItem RoleUnassignedPermissions(string roleId)
            => new($"{_prefix}role_permissions:unassigned:{roleId}");

        public static CacheItem TelegramLinkState(long chatId)
            => new($"{_prefix}tele_link_state:{chatId}", expiry: TimeSpan.FromMinutes(5));

        public static CacheItem Settings()
            => new($"{_prefix}settings", CacheMode.Memory, TimeSpan.FromHours(24));
    }
}
