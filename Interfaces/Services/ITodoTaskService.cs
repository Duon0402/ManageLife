using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        public Task<Result> CreateTodoTask(CreateTodoTaskRequest request);
        public Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request);
        public Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request);
        public Task<Result<TodoTaskModel>> GetTodoTaskById(GetTodoTaskByIdRequest request);
        public Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request);
    }
}
