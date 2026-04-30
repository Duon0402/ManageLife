using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ILanguageService
    {
        Task<Result<List<LanguageModel>>> GetListLanguagesAsync();
        Task<Result<LanguageModel>> GetLanguageByIdAsync(GetLanguageByIdRequest request);
        Task<Result<LanguageModel>> GetLanguageByCodeAsync(GetLanguageByCodeRequest request);
        Task<Result> CreateLanguageAsync(CreateLanguageRequest request);
        Task<Result> UpdateLanguageAsync(UpdateLanguageRequest request);
        Task<Result> DeleteLanguageAsync(DeleteLanguageRequest request);
        Task<Result<ChangeLanguageResult>> ChangeLanguageAsync(ChangeLanguageRequest request, string currentLanguage);
    }
}
