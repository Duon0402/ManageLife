namespace ManageLife.Contexts
{
    public interface ITranslationContext
    {
        Task<string> TranslateAsync(string key, params object[] args);
        Task<string> TranslateAsync(string key, string? languageCode, params object[] args);
    }
}
