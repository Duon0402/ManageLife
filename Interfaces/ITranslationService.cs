using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITranslationService
    {
        Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request);
        Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request);
        Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request);
        Task<Result> CreateTranslationAsync(CreateTranslationRequest request);
        Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request);
        Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request);
        Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request);
		Task<Result> ImportTranslationExcelAsync(ImportTranslationExcelRequest request);
	}
}
