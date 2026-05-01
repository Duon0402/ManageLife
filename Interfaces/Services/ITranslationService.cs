using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITranslationService
    {
        Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request, CancellationToken ct = default);
        Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request, CancellationToken ct = default);
        Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request, CancellationToken ct = default);
        Task<Result> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken ct = default);
        Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request, CancellationToken ct = default);
        Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request, CancellationToken ct = default);
        Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request, CancellationToken ct = default);
		Task<Result> ImportTranslationExcelAsync(ImportTranslationExcelRequest request, CancellationToken ct = default);
	}
}
