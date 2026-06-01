using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class FolderService : ServiceBase<FolderService>, IFolderService
    {
        private readonly IFolderRepository _folderRepo;
        private readonly IFolderFileRepository _folderFileRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IUnitOfWork _uow;

        public FolderService(
            IFolderRepository folderRepo,
            IFolderFileRepository folderFileRepo,
            IFileRepository fileRepo,
            IUnitOfWork uow,
            IAppLogger<FolderService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _folderRepo = folderRepo;
            _folderFileRepo = folderFileRepo;
            _fileRepo = fileRepo;
            _uow = uow;
        }

        public async Task<Result<List<FolderModel>>> GetFoldersAsync(CancellationToken ct = default)
        {
            try
            {
                var currentUser = _userContext.GetUserName();
                var folders = await _folderRepo.Query(true)
                    .Where(f => f.CreatedUser == currentUser)
                    .ToListAsync(ct);

                var folderIds = folders.Select(f => f.Id).ToList();
                var folderFileCounts = await _folderFileRepo.Query(true)
                    .Where(ff => folderIds.Contains(ff.FolderId))
                    .GroupBy(ff => ff.FolderId)
                    .Select(g => new { FolderId = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

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
                _logger.Error(ex, "Lỗi khi lấy danh sách folder");
                return Result.Exception<List<FolderModel>>("Lỗi khi lấy danh sách folder", ex);
            }
        }

        public async Task<Result<FolderModel>> CreateFolderAsync(CreateFolderCommand cmd, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmd.Name))
                {
                    _logger.Debug("Tên folder trống");
                    return Result.Error<FolderModel>(Result.DATA_INVALID.Code, "Tên folder không được để trống");
                }

                var entity = new FolderEntity
                {
                    Name = cmd.Name.Trim(),
                    Description = cmd.Description?.Trim()
                };

                var created = await _folderRepo.InsertAsync(entity, ct);
                if (!created)
                {
                    _logger.Debug("InsertAsync folder thất bại");
                    return Result.Error<FolderModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo folder");
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
                _logger.Error(ex, "Lỗi khi tạo folder");
                return Result.Exception<FolderModel>("Lỗi khi tạo folder", ex);
            }
        }

        public async Task<Result> DeleteFolderAsync(string folderId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _folderRepo.GetAsync(folderId, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var currentUser = _userContext.GetUserName();
                if (entity.CreatedUser != currentUser)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                await _uow.BeginTransactionAsync(ct);

                var folderFiles = await _folderFileRepo.FindAsync(ff => ff.FolderId == folderId, ct);
                if (folderFiles.Any())
                    await _folderFileRepo.BulkDeleteAsync(folderFiles, ct);

                var deleted = await _folderRepo.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    _logger.Debug("DeleteAsync folder thất bại: {0}", folderId);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá folder");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi xoá folder");
                return Result.Exception("Lỗi khi xoá folder", ex);
            }
        }

        public async Task<Result<List<FolderFileItemModel>>> GetFolderFilesAsync(string folderId, CancellationToken ct = default)
        {
            try
            {
                var folder = await _folderRepo.GetAsync(folderId, ct);
                if (folder == null)
                    return Result.Error<List<FolderFileItemModel>>(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var currentUser = _userContext.GetUserName();
                if (folder.CreatedUser != currentUser)
                    return Result.Error<List<FolderFileItemModel>>(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

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
                ).ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách file trong folder");
                return Result.Exception<List<FolderFileItemModel>>("Lỗi khi lấy danh sách file trong folder", ex);
            }
        }

        public async Task<Result> AddFileToFolderAsync(string folderId, string fileId, CancellationToken ct = default)
        {
            try
            {
                var folder = await _folderRepo.GetAsync(folderId, ct);
                if (folder == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var currentUser = _userContext.GetUserName();
                if (folder.CreatedUser != currentUser)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var existing = await _folderFileRepo.FirstOrDefaultAsync(
                    ff => ff.FolderId == folderId && ff.FileId == fileId, ct);
                if (existing != null)
                    return Result.Ok();

                var entity = new FolderFileEntity { FolderId = folderId, FileId = fileId };
                var inserted = await _folderFileRepo.InsertAsync(entity, ct);
                if (!inserted)
                {
                    _logger.Debug("InsertAsync FolderFile thất bại");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể thêm file vào folder");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi thêm file vào folder");
                return Result.Exception("Lỗi khi thêm file vào folder", ex);
            }
        }

        public async Task<Result> RemoveFileFromFolderAsync(string folderId, string fileId, CancellationToken ct = default)
        {
            try
            {
                var folder = await _folderRepo.GetAsync(folderId, ct);
                if (folder == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var currentUser = _userContext.GetUserName();
                if (folder.CreatedUser != currentUser)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var entity = await _folderFileRepo.FirstOrDefaultAsync(
                    ff => ff.FolderId == folderId && ff.FileId == fileId, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "File không tồn tại trong folder");

                var deleted = await _folderFileRepo.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    _logger.Debug("DeleteAsync FolderFile thất bại");
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá file khỏi folder");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá file khỏi folder");
                return Result.Exception("Lỗi khi xoá file khỏi folder", ex);
            }
        }
    }
}
