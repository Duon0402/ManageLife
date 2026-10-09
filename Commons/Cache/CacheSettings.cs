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

        // Phiên vừa bị đăng xuất: chặn access token còn hạn của phiên đó (giữ lâu hơn thời hạn access token 60 phút)
        public static CacheItem RevokedSession(string sessionId)
            => new($"{_prefix}revoked_session:{sessionId}", expiry: TimeSpan.FromMinutes(65));

        public static CacheItem SecurityStamp(string userId)
            => new($"{_prefix}security_stamp:{userId}", expiry: TimeSpan.FromDays(7));

        public static CacheItem RoleAssignedPermissions(string roleId)
            => new($"{_prefix}role_permissions:assigned:{roleId}");

        public static CacheItem RoleUnassignedPermissions(string roleId)
            => new($"{_prefix}role_permissions:unassigned:{roleId}");

        // Mã liên kết Telegram dùng 1 lần → userId
        public static CacheItem TelegramLinkCode(string code)
            => new($"{_prefix}tele_link_code:{code}", expiry: TimeSpan.FromMinutes(5));

        public static CacheItem TelegramBotUsername()
            => new($"{_prefix}tele_bot_username", CacheMode.Memory, TimeSpan.FromHours(24));

        // cron-job.org giới hạn số request/ngày: cache danh sách job ngắn, xoá khi ghi
        public static CacheItem CronJobs()
            => new($"{_prefix}cron_jobs", CacheMode.Memory, TimeSpan.FromSeconds(60));

        public static CacheItem Settings()
            => new($"{_prefix}settings", CacheMode.Memory, TimeSpan.FromHours(24));
    }
}
