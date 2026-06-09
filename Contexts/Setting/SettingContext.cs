using ManageLife.Commons;
using ManageLife.Interfaces;

namespace ManageLife.Contexts
{
    public class SettingContext : ISettingContext
    {
        private readonly ISettingRepository _repo;
        private readonly ICacheService _cache;

        public SettingContext(ISettingRepository repo, ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<string?> GetStringAsync(string key)
        {
            var dict = await GetDictAsync();
            return dict.TryGetValue(key, out var v) ? v : null;
        }

        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var v = await GetStringAsync(key);
            if (v == null) return defaultValue;
            return v == "true" || v == "1";
        }

        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var v = await GetStringAsync(key);
            return int.TryParse(v, out var n) ? n : defaultValue;
        }

        public async Task InvalidateCacheAsync()
            => await _cache.RemoveAsync(CacheSettings.Settings());

        private async Task<Dictionary<string, string>> GetDictAsync()
        {
            var cached = await _cache.TryGetValueAsync<Dictionary<string, string>>(CacheSettings.Settings());
            if (cached != null) return cached;

            var entities = await _repo.GetAllAsync();
            var dict = entities.ToDictionary(x => x.Key, x => x.Value);
            await _cache.SetAsync(dict, CacheSettings.Settings());
            return dict;
        }
    }
}
