using ManageLife.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace ManageLife.Services
{
    public class TranslationFileService : ITranslationFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ITranslationRepository _translationRepo;
        private readonly ILanguageRepository _languageRepo;

        public TranslationFileService(
            IWebHostEnvironment env,
            ITranslationRepository translationRepo,
            ILanguageRepository languageRepo)
        {
            _env = env;
            _translationRepo = translationRepo;
            _languageRepo = languageRepo;
        }

        private string GetTranslationsDir() =>
            Path.Combine(_env.WebRootPath, "translations");

        private string GetFilePath(string languageCode) =>
            Path.Combine(GetTranslationsDir(), $"{languageCode}.json");

        public async Task RegenerateAsync(string languageCode, CancellationToken ct = default)
        {
            var language = await _languageRepo.FirstOrDefaultAsync(
                l => l.Code == languageCode && l.IsDeleted == false, ct);

            if (language == null) return;

            var translations = await _translationRepo.Query(true)
                .Where(t => t.LanguageId == language.Id && t.IsDeleted == false)
                .ToDictionaryAsync(t => t.Key, t => t.Value, ct);

            Directory.CreateDirectory(GetTranslationsDir());

            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var json = JsonSerializer.Serialize(translations, options);
            await File.WriteAllTextAsync(GetFilePath(languageCode), json, Encoding.UTF8, ct);
        }

        public async Task RegenerateAllAsync(CancellationToken ct = default)
        {
            var codes = await _languageRepo.Query(true)
                .Where(l => l.IsDeleted == false)
                .Select(l => l.Code)
                .ToListAsync(ct);

            foreach (var code in codes)
                await RegenerateAsync(code, ct);
        }

        public async Task<Dictionary<string, string>?> ReadAsync(string languageCode, CancellationToken ct = default)
        {
            var filePath = GetFilePath(languageCode);
            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
    }
}
