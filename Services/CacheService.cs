using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

public sealed class CacheService : ICacheService
{
    private readonly IDatabase _redisDb;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IConnectionMultiplexer redis,
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _redisDb = redis.GetDatabase();
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> TryGetValueAsync<T>(CacheItem cacheItem)
    {
        try
        {
            switch (cacheItem.Mode)
            {
                case CacheMode.Redis:
                    {
                        var value = await _redisDb.StringGetAsync(cacheItem.Key);
                        return value.HasValue
                            ? JsonSerializer.Deserialize<T>(value!)
                            : default;
                    }

                case CacheMode.Memory:
                    {
                        return _memoryCache.TryGetValue(cacheItem.Key, out T? cached)
                            ? cached
                            : default;
                    }

                default:
                    return default;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache. Key={Key}, Mode={Mode}", cacheItem.Key, cacheItem.Mode);
            return default;
        }
    }

    public async Task SetAsync<T>(T value, CacheItem cacheItem)
    {
        if (value == null) return;

        try
        {
            switch (cacheItem.Mode)
            {
                case CacheMode.Redis:
                    {
                        var json = JsonSerializer.Serialize(value);
                        await _redisDb.StringSetAsync(
                            cacheItem.Key,
                            json,
                            cacheItem.Expiry);
                        break;
                    }

                case CacheMode.Memory:
                    {
                        _memoryCache.Set(
                            cacheItem.Key,
                            value,
                            cacheItem.Expiry);
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache. Key={Key}, Mode={Mode}", cacheItem.Key, cacheItem.Mode);
        }
    }

    public async Task RemoveAsync(CacheItem cacheItem)
    {
        try
        {
            switch (cacheItem.Mode)
            {
                case CacheMode.Redis:
                    await _redisDb.KeyDeleteAsync(cacheItem.Key);
                    break;

                case CacheMode.Memory:
                    _memoryCache.Remove(cacheItem.Key);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache. Key={Key}, Mode={Mode}", cacheItem.Key, cacheItem.Mode);
        }
    }

    public async Task RemoveAsync(IEnumerable<CacheItem> cacheItems)
    {
        if (cacheItems == null) return;

        try
        {
            var redisKeys = cacheItems
                .Where(x => x.Mode == CacheMode.Redis)
                .Select(x => (RedisKey)x.Key)
                .ToArray();

            if (redisKeys.Length > 0)
            {
                await _redisDb.KeyDeleteAsync(redisKeys);
            }

            foreach (var item in cacheItems.Where(x => x.Mode == CacheMode.Memory))
            {
                _memoryCache.Remove(item.Key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache batch. Count={Count}", cacheItems.Count());
        }
    }
}