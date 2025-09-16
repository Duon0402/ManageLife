using ManageLife.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

public class CacheService : ICacheService
{
    private readonly IDatabase _redisDb;
    private readonly IMemoryCache _memoryCache;
    private readonly bool _useMemoryOnly;
    private static readonly TimeSpan _defaultMemoryExpiry = TimeSpan.FromMinutes(30);

    public CacheService(
        IConnectionMultiplexer redis,
        IMemoryCache memoryCache,
        bool useMemoryOnly = false)
    {
        _redisDb = redis.GetDatabase();
        _memoryCache = memoryCache;
        _useMemoryOnly = useMemoryOnly;
    }

    public async Task<T?> TryGetValueAsync<T>(string key)
    {
        if (!_useMemoryOnly)
        {
            try
            {
                var redisValue = await _redisDb.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    return JsonSerializer.Deserialize<T>(redisValue!);
                }
            }
            catch
            {
                // Redis lỗi, fallback MemoryCache
            }
        }

        if (_memoryCache.TryGetValue(key, out T cached))
        {
            return cached;
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null) return;

        if (!_useMemoryOnly)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                var redisSucceeded = await _redisDb.StringSetAsync(key, json, expiry);

                if (redisSucceeded) return;
            }
            catch
            {
                // Redis lỗi -> fallback MemoryCache
            }
        }

        expiry ??= _defaultMemoryExpiry;
        _memoryCache.Set(key, value, expiry.Value);
    }

    public async Task RemoveAsync(string key)
    {
        if (!_useMemoryOnly)
        {
            try
            {
                await _redisDb.KeyDeleteAsync(key);
            }
            catch
            {
                // Redis lỗi -> vẫn xóa memory cache
            }
        }

        _memoryCache.Remove(key);
    }
}
