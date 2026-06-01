using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class NoteController : WebClientControllerBase
    {
        private readonly INoteService _noteService;
        private readonly INoteTagService _tagService;

        public NoteController(INoteService noteService, INoteTagService tagService)
        {
            _noteService = noteService;
            _tagService = tagService;
        }

        [AccessPagePermission]
        public IActionResult Index() => View();

        [AccessPagePermission]
        public IActionResult Edit(string id) => View("Edit", (object)id);

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<NoteModel>>> GetList(CancellationToken ct)
            => await _noteService.GetListAsync(ct);

        [HttpGet]
        [ViewPermission]
        public async Task<Result<NoteDetailModel>> GetById(string id, CancellationToken ct)
            => await _noteService.GetByIdAsync(id, ct);

        [HttpGet]
        [ViewPermission]
        public async Task<Result<NoteGraphModel>> GetGraphData(CancellationToken ct)
            => await _noteService.GetGraphDataAsync(ct);

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<NoteTagModel>>> GetTags(CancellationToken ct)
            => await _tagService.GetListAsync(ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateNoteRequest request, CancellationToken ct)
            => await _noteService.CreateAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateNoteRequest request, CancellationToken ct)
            => await _noteService.UpdateAsync(request, ct);

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] string id, CancellationToken ct)
            => await _noteService.DeleteAsync(id, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> AddLink([FromBody] AddNoteLinkRequest request, CancellationToken ct)
            => await _noteService.AddLinkAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> RemoveLink([FromBody] RemoveNoteLinkRequest request, CancellationToken ct)
            => await _noteService.RemoveLinkAsync(request, ct);
    }
}
