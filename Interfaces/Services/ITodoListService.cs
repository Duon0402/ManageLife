using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoListService
    {
        Task<Result<List<TodoListModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result<TodoListModel>> CreateAsync(SaveTodoListRequest request, CancellationToken ct = default);
        Task<Result<TodoListModel>> UpdateAsync(string id, SaveTodoListRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> ReorderAsync(ReorderRequest request, CancellationToken ct = default);
    }
}
