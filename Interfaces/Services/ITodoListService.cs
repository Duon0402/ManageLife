using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoListService
    {
        Task<Result> CreateAsync(SaveTodoListRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(string id, SaveTodoListRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> ReorderAsync(ReorderRequest request, CancellationToken ct = default);
    }
}
