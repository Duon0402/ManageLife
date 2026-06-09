namespace ManageLife.Contexts
{
    public interface ISettingContext
    {
        Task<string?> GetStringAsync(string key);
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);
        Task<int> GetIntAsync(string key, int defaultValue = 0);
        Task InvalidateCacheAsync();
    }
}
