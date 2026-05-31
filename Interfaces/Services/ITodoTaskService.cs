using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        public Task<Result> CreateTodoTask(CreateTodoTaskRequest request, CancellationToken ct = default);
        public Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request, CancellationToken ct = default);
        public Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request, CancellationToken ct = default);
        public Task<Result> ChangeTaskStatus(ChangeTaskStatusRequest request, CancellationToken ct = default);
        public Task<Result<TodoTaskModel>> GetTodoTaskById(GetTodoTaskByIdRequest request, CancellationToken ct = default);
        public Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request, CancellationToken ct = default);
        public Task<Result<List<TodoTaskModel>>> GetTodayTasks(CancellationToken ct = default);
    }
}
