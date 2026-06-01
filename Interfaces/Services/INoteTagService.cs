using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface INoteTagService
    {
        Task<Result<List<NoteTagModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateNoteTagRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateNoteTagRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
    }
}
