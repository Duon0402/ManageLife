using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface INoteService
    {
        Task<Result<List<NoteModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result<NoteDetailModel>> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Result> CreateAsync(CreateNoteRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateNoteRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> AddLinkAsync(AddNoteLinkRequest request, CancellationToken ct = default);
        Task<Result> RemoveLinkAsync(RemoveNoteLinkRequest request, CancellationToken ct = default);
        Task<Result<NoteGraphModel>> GetGraphDataAsync(CancellationToken ct = default);
    }
}
