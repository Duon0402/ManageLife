using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoListService
    {
        Task<Result> CreateAsync(CreateToDoListRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateToDoListRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteToDoListRequest request, CancellationToken ct = default);
        Task<Result<List<TodoListModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result<TodoListModel>> GetByIdAsync(GetTodoListByIdRequest request, CancellationToken ct = default);
    }
}
