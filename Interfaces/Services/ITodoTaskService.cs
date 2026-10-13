using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        Task<Result<TodoSyncModel>> GetChangesAsync(DateTime? since, CancellationToken ct = default);
        Task<Result> CreateAsync(SaveTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(string id, SaveTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> SetCompletedAsync(string id, bool completed, CancellationToken ct = default);
        Task<Result> ReorderAsync(ReorderRequest request, CancellationToken ct = default);
        Task<Result> AddChecklistItemAsync(string taskId, SaveChecklistItemRequest request, CancellationToken ct = default);
        Task<Result> UpdateChecklistItemAsync(string itemId, SaveChecklistItemRequest request, CancellationToken ct = default);
        Task<Result> DeleteChecklistItemAsync(string itemId, CancellationToken ct = default);
    }
}
