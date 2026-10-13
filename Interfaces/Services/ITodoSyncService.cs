using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoSyncService
    {
        /// <summary>Áp dụng lần lượt các thao tác app làm trên máy rồi trả thay đổi từ mốc `Since`.</summary>
        Task<Result<TodoSyncModel>> SyncAsync(TodoSyncRequest request, CancellationToken ct = default);
    }
}
