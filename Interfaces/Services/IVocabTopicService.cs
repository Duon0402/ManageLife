using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IVocabTopicService
    {
        Task<Result<List<VocabTopicModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateVocabTopicRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateVocabTopicRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
    }
}
