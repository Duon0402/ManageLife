using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        Task<Result<PageList<TodoTaskModel>>> GetListAsync(GetTodoTasksRequest request, CancellationToken ct = default);
        Task<Result<TodoTaskDetailModel>> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Result<TodoTaskDetailModel>> CreateAsync(SaveTodoTaskRequest request, CancellationToken ct = default);
        Task<Result<TodoTaskDetailModel>> UpdateAsync(string id, SaveTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result<TodoTaskModel>> SetCompletedAsync(string id, bool completed, CancellationToken ct = default);
        Task<Result> ReorderAsync(ReorderRequest request, CancellationToken ct = default);
        Task<Result<TodoChecklistItemModel>> AddChecklistItemAsync(string taskId, SaveChecklistItemRequest request, CancellationToken ct = default);
        Task<Result<TodoChecklistItemModel>> UpdateChecklistItemAsync(string itemId, SaveChecklistItemRequest request, CancellationToken ct = default);
        Task<Result> DeleteChecklistItemAsync(string itemId, CancellationToken ct = default);
        Task<Result<TodoSummaryModel>> GetSummaryAsync(CancellationToken ct = default);
    }
}
