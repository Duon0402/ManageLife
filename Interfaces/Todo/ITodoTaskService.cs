using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoTaskService
    {
        public Task<Result> CreateTodoTask(CreateTodoTaskRequest request);
        public Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request);
        public Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request);
        public Task<Result<TodoListModel>> GetTodoTaskById(GetTodoTaskByIdRequest request);
        public Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request);
    }
}
