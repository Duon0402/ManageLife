using ManageLife.Core;
using ManageLife.Models;
using ManageLife.Models.ShortUrl;

namespace ManageLife.Interfaces
{
    public interface IShortUrlService
    {
        Task<Result<List<ShortUrlModel>>> GetListAsync(CancellationToken ct);
        Task<Result<ShortUrlModel>> GetByCodeAsync(GetShortUrlByCodeRequest request, CancellationToken ct);
        Task<Result> CreateAsync(CreateShortUrlRequest request, CancellationToken ct);
        Task<Result> DeleteAsync(DeleteShortUrlRequest request, CancellationToken ct);
        Task<Result> RecordClickAsync(RecordShortUrlClickRequest request, CancellationToken ct);
    }
}
