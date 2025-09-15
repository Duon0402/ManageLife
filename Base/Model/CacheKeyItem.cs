namespace ManageLife.Base
{
    public class CacheKeyItem
    {
        public CacheKeyItem(string key, TimeSpan? expiry = null)
        {
            Key = key;
            Expiry = expiry;
        }

        public string Key { get; set; }
        public TimeSpan? Expiry { get; set; }
    }
}
