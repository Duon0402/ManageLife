using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepo;
        private readonly IAlbumFileRepository _albumFileRepo;
        private readonly IFileRepository _fileRepo;

        public AlbumService(IAlbumRepository albumRepo, IAlbumFileRepository albumFileRepo, IFileRepository fileRepo)
        {
            _albumRepo = albumRepo;
            _albumFileRepo = albumFileRepo;
            _fileRepo = fileRepo;
        }

        public async Task<Result<AlbumEntity>> CreateAlbumAsync(string title, string? description, string? coverPhotoId)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result.Error<AlbumEntity>(Result.DATA_INVALID.Code, "Title cannot be empty");

            var album = new AlbumEntity
            {
                Id = IdHeper.NewId(),
                Title = title,
                Description = description,
                CoverPhotoId = coverPhotoId
            };

            var success = await _albumRepo.InsertAsync(album);
            if (!success)
            {
                return Result.Error<AlbumEntity>(Result.DATA_NOT_CREATE.Code, "Failed to create album");
            }

            return Result.Ok(album);
        }

        public async Task<Result> DeleteAlbumAsync(string id)
        {
            var album = await _albumRepo.GetAsync(id);
            if (album == null)
            {
                return Result.Error(Result.DATA_NOT_EXISTED.Code, "Album not found");
            }

            await _albumRepo.DeleteAsync(album);
            return Result.Ok();
        }

        public async Task<Result<AlbumEntity>> GetAlbumAsync(string id)
        {
            var album = await _albumRepo.GetAsync(id);
            if (album == null)
            {
                return Result.Error<AlbumEntity>(Result.DATA_NOT_EXISTED.Code, "Album not found");
            }

            return Result.Ok(album);
        }

        public async Task<Result<IEnumerable<FileEntity>>> GetAlbumFilesAsync(string albumId)
        {
            var albumFiles = await _albumFileRepo.FindAsync(x => x.AlbumId == albumId);
            var fileIds = albumFiles.Select(x => x.FileId).ToList();

            if (!fileIds.Any())
            {
                return Result.Ok(Enumerable.Empty<FileEntity>());
            }

            var files = await _fileRepo.FindAsync(x => fileIds.Contains(x.Id));
            return Result.Ok(files);
        }

        public async Task<Result<IEnumerable<AlbumEntity>>> GetAllAlbumsAsync()
        {
            var albums = await _albumRepo.GetAllAsync();
            return Result.Ok(albums.OrderByDescending(x => x.CreatedTime).AsEnumerable());
        }

        public async Task<Result> LinkFileToAlbumAsync(string albumId, string fileId)
        {
            var album = await _albumRepo.GetAsync(albumId);
            if (album == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, "Album not found");

            var file = await _fileRepo.GetAsync(fileId);
            if (file == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, "File not found");

            var existingLink = await _albumFileRepo.FirstOrDefaultAsync(x => x.AlbumId == albumId && x.FileId == fileId);
            if (existingLink != null)
            {
                return Result.Ok();
            }

            var link = new AlbumFileEntity
            {
                Id = IdHeper.NewId(),
                AlbumId = albumId,
                FileId = fileId
            };

            await _albumFileRepo.InsertAsync(link);

            if (album.CoverPhotoId.IsEmpty())
            {
                album.CoverPhotoId = fileId;
                await _albumRepo.UpdateAsync(album);
            }

            return Result.Ok();
        }

        public async Task<Result> UnlinkFileFromAlbumAsync(string albumId, string fileId)
        {
            var link = await _albumFileRepo.FirstOrDefaultAsync(x => x.AlbumId == albumId && x.FileId == fileId);
            if (link != null)
            {
                await _albumFileRepo.DeleteAsync(link);
            }
            return Result.Ok();
        }
    }
}
