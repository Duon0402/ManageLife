namespace ManageLife.Interfaces
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> TryGetValueAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
