using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        Task<Result> CreateAsync(CreateTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteTodoTaskRequest request, CancellationToken ct = default);
        Task<Result> ChangeStatusAsync(ChangeTaskStatusRequest request, CancellationToken ct = default);
        Task<Result<TodoTaskModel>> GetByIdAsync(GetTodoTaskByIdRequest request, CancellationToken ct = default);
        Task<Result<List<TodoTaskModel>>> GetListAsync(GetListTodoTasksRequest request, CancellationToken ct = default);
        Task<Result<List<TodoTaskModel>>> GetTodayTasksAsync(CancellationToken ct = default);
    }
}
