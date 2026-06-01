using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ICodeSequenceService
    {
        Task<Result<List<CodeSequenceModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateCodeSequenceRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateCodeSequenceRequest request, CancellationToken ct = default);
        Task<Result> ResetAsync(ResetCodeSequenceRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteCodeSequenceRequest request, CancellationToken ct = default);
    }
}
