using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class NoteTagController : WebClientControllerBase
    {
        private readonly INoteTagService _service;

        public NoteTagController(INoteTagService service)
        {
            _service = service;
        }

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<NoteTagModel>>> GetList(CancellationToken ct)
            => await _service.GetListAsync(ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateNoteTagRequest request, CancellationToken ct)
            => await _service.CreateAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateNoteTagRequest request, CancellationToken ct)
            => await _service.UpdateAsync(request, ct);

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] string id, CancellationToken ct)
            => await _service.DeleteAsync(id, ct);
    }
}
