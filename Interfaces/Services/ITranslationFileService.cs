namespace ManageLife.Interfaces
{
    public interface ITranslationFileService
    {
        Task RegenerateAsync(string languageCode, CancellationToken ct = default);
        Task RegenerateAllAsync(CancellationToken ct = default);
        Task<Dictionary<string, string>?> ReadAsync(string languageCode, CancellationToken ct = default);
    }
}
