using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ILanguageService
    {
        public Task<Result<List<LanguageModel>>> GetListLanguagesAsync();
        public Task<Result<LanguageModel>> GetLanguageByIdAsync(GetLanguageByIdRequest request);
        public Task<Result<LanguageModel>> GetLanguageByCodeAsync(GetLanguageByCodeRequest request);
        public Task<Result> CreateLanguageAsync(CreateLanguageRequest request);
        public Task<Result> UpdateLanguageAsync(UpdateLanguageRequest request);
        public Task<Result> DeleteLanguageAsync(DeleteLanguageRequest request);
    }
}
