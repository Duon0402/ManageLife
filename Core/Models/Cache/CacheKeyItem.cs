namespace ManageLife.Core
{
    public sealed class CacheItem
    {
        private static readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(60);

        public string Key { get; }
        public CacheMode Mode { get; }
        public TimeSpan Expiry { get; }

        public CacheItem(string key, CacheMode mode = CacheMode.Redis, TimeSpan? expiry = null)
        {
            Key = key;
            Mode = mode;
            Expiry = expiry ?? _defaultExpiry;
        }
    }
}
