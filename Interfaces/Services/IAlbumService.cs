using ManageLife.Base;
using ManageLife.Entities;

namespace ManageLife.Interfaces
{
    public interface IAlbumService
    {
        Task<Result<AlbumEntity>> CreateAlbumAsync(string title, string? description, string? coverPhotoId);
        Task<Result<AlbumEntity>> GetAlbumAsync(string id);
        Task<Result<IEnumerable<AlbumEntity>>> GetAllAlbumsAsync();
        Task<Result> DeleteAlbumAsync(string id);
        Task<Result> LinkFileToAlbumAsync(string albumId, string fileId);
        Task<Result<IEnumerable<FileEntity>>> GetAlbumFilesAsync(string albumId);
        Task<Result> UnlinkFileFromAlbumAsync(string albumId, string fileId);
    }
}
