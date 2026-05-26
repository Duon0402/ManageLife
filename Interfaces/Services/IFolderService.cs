using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IFolderService
    {
        Task<Result<List<FolderModel>>> GetFoldersAsync(CancellationToken ct = default);
        Task<Result<FolderModel>> CreateFolderAsync(CreateFolderCommand cmd, CancellationToken ct = default);
        Task<Result> DeleteFolderAsync(string folderId, CancellationToken ct = default);
        Task<Result<List<FolderFileItemModel>>> GetFolderFilesAsync(string folderId, CancellationToken ct = default);
        Task<Result> AddFileToFolderAsync(string folderId, string fileId, CancellationToken ct = default);
        Task<Result> RemoveFileFromFolderAsync(string folderId, string fileId, CancellationToken ct = default);
    }
}
