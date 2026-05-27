using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepo;
        private readonly IFolderFileRepository _folderFileRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IAppLogger<FolderService> _logger;

        public FolderService(
            IFolderRepository folderRepo,
            IFolderFileRepository folderFileRepo,
            IFileRepository fileRepo,
            IAppLogger<FolderService> logger)
        {
            _folderRepo = folderRepo;
            _folderFileRepo = folderFileRepo;
            _fileRepo = fileRepo;
            _logger = logger;
        }

        public async Task<Result<List<FolderModel>>> GetFoldersAsync(CancellationToken ct = default)
        {
            try
            {
                var folders = await _folderRepo.Query(true).ToListAsync();

                var folderIds = folders.Select(f => f.Id).ToList();
                var folderFileCounts = await _folderFileRepo.Query(true)
                    .Where(ff => folderIds.Contains(ff.FolderId))
                    .GroupBy(ff => ff.FolderId)
                    .Select(g => new { FolderId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var countDict = folderFileCounts.ToDictionary(x => x.FolderId, x => x.Count);

                var models = folders
                    .OrderByDescending(f => f.CreatedTime)
                    .Select(f => new FolderModel
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Description = f.Description,
                        CreatedTime = f.CreatedTime,
                        CreatedUser = f.CreatedUser,
                        PhotoCount = countDict.GetValueOrDefault(f.Id, 0)
                    })
                    .ToList();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception<List<FolderModel>>("Lỗi khi lấy danh sách folder", ex);
            }
        }

        public async Task<Result<FolderModel>> CreateFolderAsync(CreateFolderCommand cmd, CancellationToken ct = default)
        {
            string msg;
            try
            {
                if (string.IsNullOrWhiteSpace(cmd.Name))
                {
                    msg = "Tên folder không được để trống";
                    _logger.Debug(msg);
                    return Result.Error<FolderModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = new FolderEntity
                {
                    Name = cmd.Name.Trim(),
                    Description = cmd.Description?.Trim()
                };

                var b = await _folderRepo.InsertAsync(entity);
                if (!b)
                {
                    msg = "Không thể tạo folder";
                    _logger.Debug(msg);
                    return Result.Error<FolderModel>(Result.DATA_NOT_CREATE.Code, msg);
                }

                var model = new FolderModel
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    CreatedTime = entity.CreatedTime,
                    CreatedUser = entity.CreatedUser,
                    PhotoCount = 0
                };

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception<FolderModel>("Lỗi khi tạo folder", ex);
            }
        }

        public async Task<Result> DeleteFolderAsync(string folderId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var entity = await _folderRepo.GetAsync(folderId);
                if (entity == null)
                {
                    msg = "Folder không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                // Xoá tất cả link folder-file trước
                var folderFiles = await _folderFileRepo.FindAsync(ff => ff.FolderId == folderId);
                if (folderFiles.Any())
                    await _folderFileRepo.BulkDeleteAsync(folderFiles);

                var b = await _folderRepo.DeleteAsync(entity);
                if (!b)
                {
                    msg = "Không thể xoá folder";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception("Lỗi khi xoá folder", ex);
            }
        }

        public async Task<Result<List<FolderFileItemModel>>> GetFolderFilesAsync(string folderId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var folder = await _folderRepo.GetAsync(folderId);
                if (folder == null)
                {
                    msg = "Folder không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error<List<FolderFileItemModel>>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                // Single join query – avoids two separate round-trips
                var models = await (
                    from ff in _folderFileRepo.Query(true)
                    join f in _fileRepo.Query(true) on ff.FileId equals f.Id
                    where ff.FolderId == folderId
                    select new FolderFileItemModel
                    {
                        FileId = f.Id,
                        FileName = f.FileName,
                        FileUrl = $"/FileStorage/GetFile?fileId={f.Id}"
                    }
                ).ToListAsync();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception<List<FolderFileItemModel>>("Lỗi khi lấy danh sách file trong folder", ex);
            }
        }

        public async Task<Result> AddFileToFolderAsync(string folderId, string fileId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                // Kiểm tra folder tồn tại
                var folder = await _folderRepo.GetAsync(folderId);
                if (folder == null)
                {
                    msg = "Folder không tồn tại";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                // Kiểm tra đã link chưa
                var existing = await _folderFileRepo.FirstOrDefaultAsync(
                    ff => ff.FolderId == folderId && ff.FileId == fileId);
                if (existing != null)
                    return Result.Ok(); // đã tồn tại thì bỏ qua

                var entity = new FolderFileEntity
                {
                    FolderId = folderId,
                    FileId = fileId
                };

                var b = await _folderFileRepo.InsertAsync(entity);
                if (!b)
                {
                    msg = "Không thể thêm file vào folder";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception("Lỗi khi thêm file vào folder", ex);
            }
        }

        public async Task<Result> RemoveFileFromFolderAsync(string folderId, string fileId, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var entity = await _folderFileRepo.FirstOrDefaultAsync(
                    ff => ff.FolderId == folderId && ff.FileId == fileId);
                if (entity == null)
                {
                    msg = "File không tồn tại trong folder";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var b = await _folderFileRepo.DeleteAsync(entity);
                if (!b)
                {
                    msg = "Không thể xoá file khỏi folder";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return Result.Exception("Lỗi khi xoá file khỏi folder", ex);
            }
        }
    }
}
