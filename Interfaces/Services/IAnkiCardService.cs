using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IAnkiCardService
    {
        Task<Result<List<AnkiCardModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateAnkiCardRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateAnkiCardRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result<List<AnkiCardModel>>> GetAllForExportAsync(CancellationToken ct = default);
    }
}
