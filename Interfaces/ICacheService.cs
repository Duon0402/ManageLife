namespace ManageLife.Interfaces
{
    public interface ICacheService
    {
        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        public Task<T?> TryGetValueAsync<T>(string key);
        public Task RemoveAsync(string key);
    }
}
