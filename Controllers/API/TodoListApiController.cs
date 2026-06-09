using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/todo-list")]
    public class TodoListApiController : ApiControllerBase
    {
        private readonly ITodoListService _service;

        public TodoListApiController(ITodoListService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(CancellationToken ct)
        {
            var rs = await _service.GetListTodoLists(ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            return BadRequest(rs.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var rs = await _service.GetTodoListById(new GetTodoListByIdRequest { Id = id }, ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateToDoListRequest request, CancellationToken ct)
        {
            var rs = await _service.CreateToDoList(request, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_EXISTED.Code)
                return Conflict(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateToDoListRequest request, CancellationToken ct)
        {
            var rs = await _service.UpdateToDoList(request, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            if (rs.Code == Result.DATA_EXISTED.Code)
                return Conflict(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var rs = await _service.DeleteToDoList(new DeleteToDoListRequest { Id = id }, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }
    }
}
