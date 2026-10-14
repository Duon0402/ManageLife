using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IFinanceSyncService
    {
        /// <summary>Áp dụng lần lượt các thao tác app làm trên máy rồi trả thay đổi từ mốc `Since`.</summary>
        Task<Result<FinanceSyncModel>> SyncAsync(FinanceSyncRequest request, CancellationToken ct = default);
    }
}
