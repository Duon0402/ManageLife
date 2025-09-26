using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITranslationService
    {
        public Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request);
        public Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request);
        public Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request);
        public Task<Result> CreateTranslationAsync(CreateTranslationRequest request);
        public Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request);
        public Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request);
        public Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request);
    }
}
