using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IVocabWordService
    {
        Task<Result<List<VocabWordModel>>> GetListAsync(GetListVocabWordsRequest request, CancellationToken ct = default);
        Task<Result<VocabWordModel>> GetByIdAsync(GetVocabWordByIdRequest request, CancellationToken ct = default);
        Task<Result> CreateAsync(CreateVocabWordRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateVocabWordRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteVocabWordRequest request, CancellationToken ct = default);
        Task<Result<DictionaryLookupResult>> LookupFromDictionaryAsync(LookupWordRequest request, CancellationToken ct = default);
    }
}
