using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Admin
{
    public class CodeSequenceController : WebAdminControllerBase
    {
        private readonly ICodeSequenceService _service;

        public CodeSequenceController(ICodeSequenceService service)
        {
            _service = service;
        }

        [AccessPagePermission]
        public IActionResult Index() => View();

        [HttpGet]
        [ViewPermission]
        public async Task<Result<List<CodeSequenceModel>>> GetList(CancellationToken ct)
            => await _service.GetListAsync(ct);

        [HttpPost]
        [InsertPermission]
        public async Task<Result> Create([FromBody] CreateCodeSequenceRequest request, CancellationToken ct)
            => await _service.CreateAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Update([FromBody] UpdateCodeSequenceRequest request, CancellationToken ct)
            => await _service.UpdateAsync(request, ct);

        [HttpPost]
        [UpdatePermission]
        public async Task<Result> Reset([FromBody] ResetCodeSequenceRequest request, CancellationToken ct)
            => await _service.ResetAsync(request, ct);

        [HttpPost]
        [DeletePermission]
        public async Task<Result> Delete([FromBody] DeleteCodeSequenceRequest request, CancellationToken ct)
            => await _service.DeleteAsync(request, ct);
    }
}
