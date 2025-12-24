namespace ManageLife.Contexts
{
    public interface ILanguageContext
    {
        string GetCurrentLanguage();
        void SetCurrentLanguage(string lang);
        Task<string> GetCurrentLanguageNameAsync(string? languageCode = null);
    }
}
