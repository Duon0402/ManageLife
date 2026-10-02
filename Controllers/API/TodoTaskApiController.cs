using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/todo")]
    public class TodoTaskApiController : ApiControllerBase
    {
        private readonly ITodoTaskService _taskService;

        public TodoTaskApiController(ITodoTaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("tasks")]
        public async Task<Result<PageList<TodoTaskModel>>> GetList([FromQuery] GetTodoTasksRequest request, CancellationToken ct)
        {
            return await _taskService.GetListAsync(request, ct);
        }

        [HttpGet("tasks/{id}")]
        public async Task<Result<TodoTaskDetailModel>> GetById(string id, CancellationToken ct)
        {
            return await _taskService.GetByIdAsync(id, ct);
        }

        [HttpPost("tasks")]
        public async Task<Result<TodoTaskDetailModel>> Create([FromBody] SaveTodoTaskRequest request, CancellationToken ct)
        {
            return await _taskService.CreateAsync(request, ct);
        }

        [HttpPut("tasks/{id}")]
        public async Task<Result<TodoTaskDetailModel>> Update(string id, [FromBody] SaveTodoTaskRequest request, CancellationToken ct)
        {
            return await _taskService.UpdateAsync(id, request, ct);
        }

        [HttpDelete("tasks/{id}")]
        public async Task<Result> Delete(string id, CancellationToken ct)
        {
            return await _taskService.DeleteAsync(id, ct);
        }

        [HttpPost("tasks/{id}/complete")]
        public async Task<Result<TodoTaskModel>> Complete(string id, CancellationToken ct)
        {
            return await _taskService.SetCompletedAsync(id, true, ct);
        }

        [HttpPost("tasks/{id}/uncomplete")]
        public async Task<Result<TodoTaskModel>> Uncomplete(string id, CancellationToken ct)
        {
            return await _taskService.SetCompletedAsync(id, false, ct);
        }

        [HttpPut("tasks/reorder")]
        public async Task<Result> Reorder([FromBody] ReorderRequest request, CancellationToken ct)
        {
            return await _taskService.ReorderAsync(request, ct);
        }

        [HttpPost("tasks/{id}/checklist")]
        public async Task<Result<TodoChecklistItemModel>> AddChecklistItem(string id, [FromBody] SaveChecklistItemRequest request, CancellationToken ct)
        {
            return await _taskService.AddChecklistItemAsync(id, request, ct);
        }

        [HttpPut("checklist/{itemId}")]
        public async Task<Result<TodoChecklistItemModel>> UpdateChecklistItem(string itemId, [FromBody] SaveChecklistItemRequest request, CancellationToken ct)
        {
            return await _taskService.UpdateChecklistItemAsync(itemId, request, ct);
        }

        [HttpDelete("checklist/{itemId}")]
        public async Task<Result> DeleteChecklistItem(string itemId, CancellationToken ct)
        {
            return await _taskService.DeleteChecklistItemAsync(itemId, ct);
        }

        [HttpGet("summary")]
        public async Task<Result<TodoSummaryModel>> GetSummary(CancellationToken ct)
        {
            return await _taskService.GetSummaryAsync(ct);
        }
    }
}
