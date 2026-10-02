using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/todo/lists")]
    public class TodoListApiController : ApiControllerBase
    {
        private readonly ITodoListService _listService;

        public TodoListApiController(ITodoListService listService)
        {
            _listService = listService;
        }

        [HttpGet]
        public async Task<Result<List<TodoListModel>>> GetList(CancellationToken ct)
        {
            return await _listService.GetListAsync(ct);
        }

        [HttpPost]
        public async Task<Result<TodoListModel>> Create([FromBody] SaveTodoListRequest request, CancellationToken ct)
        {
            return await _listService.CreateAsync(request, ct);
        }

        [HttpPut("{id}")]
        public async Task<Result<TodoListModel>> Update(string id, [FromBody] SaveTodoListRequest request, CancellationToken ct)
        {
            return await _listService.UpdateAsync(id, request, ct);
        }

        /// <summary>Xoá list cùng toàn bộ task của list.</summary>
        [HttpDelete("{id}")]
        public async Task<Result> Delete(string id, CancellationToken ct)
        {
            return await _listService.DeleteAsync(id, ct);
        }

        [HttpPut("reorder")]
        public async Task<Result> Reorder([FromBody] ReorderRequest request, CancellationToken ct)
        {
            return await _listService.ReorderAsync(request, ct);
        }
    }
}
