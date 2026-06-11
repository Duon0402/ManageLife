using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Authorize]
    [Route("api/todo-task")]
    public class TodoTaskApiController : ApiControllerBase
    {
        private readonly ITodoTaskService _service;

        public TodoTaskApiController(ITodoTaskService service)
        {
            _service = service;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday(CancellationToken ct)
        {
            var rs = await _service.GetTodayTasksAsync(ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            return BadRequest(rs.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetListTodoTasksRequest request, CancellationToken ct)
        {
            var rs = await _service.GetListAsync(request, ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            return BadRequest(rs.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var rs = await _service.GetByIdAsync(new GetTodoTaskByIdRequest { Id = id }, ct);
            if (rs.IsOk())
                return Ok(rs.Data);

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoTaskRequest request, CancellationToken ct)
        {
            var rs = await _service.CreateAsync(request, ct);
            if (rs.IsOk())
                return Ok();

            return BadRequest(rs.Message);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTodoTaskRequest request, CancellationToken ct)
        {
            var rs = await _service.UpdateAsync(request, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeTaskStatusRequest request, CancellationToken ct)
        {
            request.Id = id;
            var rs = await _service.ChangeStatusAsync(request, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var rs = await _service.DeleteAsync(new DeleteTodoTaskRequest { Id = id }, ct);
            if (rs.IsOk())
                return Ok();

            if (rs.Code == Result.DATA_NOT_EXISTED.Code)
                return NotFound(rs.Message);

            return BadRequest(rs.Message);
        }
    }
}
