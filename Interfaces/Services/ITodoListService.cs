using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoListService
    {
        public Task<Result> CreateToDoList(CreateToDoListRequest request);
        public Task<Result> UpdateToDoList(UpdateToDoListRequest request);
        public Task<Result> DeleteToDoList(DeleteToDoListRequest request);
        public Task<Result<List<TodoListModel>>> GetListTodoLists();
        public Task<Result<TodoListModel>> GetTodoListById(GetTodoListByIdRequest request);
    }
}
