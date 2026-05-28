using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IVocabDeckService
    {
        Task<Result<List<VocabDeckModel>>> GetListAsync(string? topicId, CancellationToken ct = default);
        Task<Result<VocabDeckModel>> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Result> CreateAsync(CreateVocabDeckRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateVocabDeckRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> AddWordAsync(AddWordToDeckRequest request, CancellationToken ct = default);
        Task<Result> RemoveWordAsync(string deckId, string wordId, CancellationToken ct = default);
        Task<Result<List<VocabWordModel>>> GetWordsAsync(string deckId, CancellationToken ct = default);
    }
}
