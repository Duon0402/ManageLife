using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IFolderService
    {
        Task<Result<List<FolderModel>>> GetFoldersAsync();
        Task<Result<FolderModel>> CreateFolderAsync(CreateFolderCommand cmd);
        Task<Result> DeleteFolderAsync(string folderId);
        Task<Result<List<FolderFileItemModel>>> GetFolderFilesAsync(string folderId);
        Task<Result> AddFileToFolderAsync(string folderId, string fileId);
        Task<Result> RemoveFileFromFolderAsync(string folderId, string fileId);
    }
}
