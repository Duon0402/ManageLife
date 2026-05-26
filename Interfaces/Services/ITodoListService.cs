using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITodoListService
    {
        public Task<Result> CreateToDoList(CreateToDoListRequest request, CancellationToken ct = default);
        public Task<Result> UpdateToDoList(UpdateToDoListRequest request, CancellationToken ct = default);
        public Task<Result> DeleteToDoList(DeleteToDoListRequest request, CancellationToken ct = default);
        public Task<Result<List<TodoListModel>>> GetListTodoLists(CancellationToken ct = default);
        public Task<Result<TodoListModel>> GetTodoListById(GetTodoListByIdRequest request, CancellationToken ct = default);
    }
}
