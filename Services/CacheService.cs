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
            return cached;

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null) return;

        var memoryExpiry = expiry ?? (_useMemoryOnly ? _defaultMemoryExpiry : null);

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

        if (memoryExpiry.HasValue)
            _memoryCache.Set(key, value, memoryExpiry.Value);
        else
            _memoryCache.Set(key, value);
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
                // Redis lỗi -> vẫn xóa MemoryCache
            }
        }

        _memoryCache.Remove(key);
    }
}
