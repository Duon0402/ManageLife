using ManageLife.Base;

namespace ManageLife.Interfaces
{
    public interface ICacheService
    {
        Task SetAsync<T>(T value, CacheItem cacheKeyItem);
        Task<T?> TryGetValueAsync<T>(CacheItem cacheItem);
        Task RemoveAsync(CacheItem cacheItem);
        Task RemoveAsync(IEnumerable<CacheItem> cacheItems);
    }
}
