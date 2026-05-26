using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ILanguageService
    {
        Task<Result<List<LanguageModel>>> GetListLanguagesAsync(CancellationToken ct = default);
        Task<Result<LanguageModel>> GetLanguageByIdAsync(GetLanguageByIdRequest request, CancellationToken ct = default);
        Task<Result<LanguageModel>> GetLanguageByCodeAsync(GetLanguageByCodeRequest request, CancellationToken ct = default);
        Task<Result> CreateLanguageAsync(CreateLanguageRequest request, CancellationToken ct = default);
        Task<Result> UpdateLanguageAsync(UpdateLanguageRequest request, CancellationToken ct = default);
        Task<Result> DeleteLanguageAsync(DeleteLanguageRequest request, CancellationToken ct = default);
        Task<Result<ChangeLanguageResult>> ChangeLanguageAsync(ChangeLanguageRequest request, string currentLanguage, CancellationToken ct = default);
    }
}
