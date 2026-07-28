using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class TodoController : WebClientControllerBase
    {
        private readonly ITodoTaskService _taskService;
        private readonly ITodoListService _listService;
        private readonly ISettingContext _settingContext;

        public TodoController(ITodoTaskService taskService, ITodoListService listService, ISettingContext settingContext)
        {
            _taskService = taskService;
            _listService = listService;
            _settingContext = settingContext;
        }

        [AccessPagePermission]
        public async Task<IActionResult> Index()
        {
            if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableTodo, true))
                return NotFound();
            return View();
        }

        [AccessPagePermission]
        public IActionResult All() => View();

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<TodoTaskModel>>> GetTodayTasks(CancellationToken ct)
            => await _taskService.GetTodayTasksAsync(ct);

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<TodoTaskModel>>> GetList([FromQuery] GetListTodoTasksRequest request, CancellationToken ct)
            => await _taskService.GetListAsync(request, ct);

        [ViewPermission]
        [HttpGet]
        public async Task<Result<List<TodoListModel>>> GetLists(CancellationToken ct)
            => await _listService.GetListAsync(ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateTask([FromBody] CreateTodoTaskRequest request, CancellationToken ct)
            => await _taskService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> UpdateTask([FromBody] UpdateTodoTaskRequest request, CancellationToken ct)
            => await _taskService.UpdateAsync(request, ct);

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> ChangeStatus([FromBody] ChangeTaskStatusRequest request, CancellationToken ct)
            => await _taskService.ChangeStatusAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> DeleteTask(string id, CancellationToken ct)
            => await _taskService.DeleteAsync(new DeleteTodoTaskRequest { Id = id }, ct);

        [InsertPermission]
        [HttpPost]
        public async Task<Result> CreateList([FromBody] CreateToDoListRequest request, CancellationToken ct)
            => await _listService.CreateAsync(request, ct);

        [UpdatePermission]
        [HttpPost]
        public async Task<Result> UpdateList([FromBody] UpdateToDoListRequest request, CancellationToken ct)
            => await _listService.UpdateAsync(request, ct);

        [DeletePermission]
        [HttpDelete]
        public async Task<Result> DeleteList(string id, CancellationToken ct)
            => await _listService.DeleteAsync(new DeleteToDoListRequest { Id = id }, ct);
    }
}
