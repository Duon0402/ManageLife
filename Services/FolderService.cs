using ManageLife.Base;
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

        public FolderService(
            IFolderRepository folderRepo,
            IFolderFileRepository folderFileRepo,
            IFileRepository fileRepo)
        {
            _folderRepo = folderRepo;
            _folderFileRepo = folderFileRepo;
            _fileRepo = fileRepo;
        }

        public async Task<Result<List<FolderModel>>> GetFoldersAsync()
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
                return Result.Exception<List<FolderModel>>("Lỗi khi lấy danh sách folder", ex);
            }
        }

        public async Task<Result<FolderModel>> CreateFolderAsync(CreateFolderCommand cmd)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmd.Name))
                    return Result.Error<FolderModel>(Result.DATA_INVALID.Code, "Tên folder không được để trống");

                var entity = new FolderEntity
                {
                    Name = cmd.Name.Trim(),
                    Description = cmd.Description?.Trim()
                };

                var b = await _folderRepo.InsertAsync(entity);
                if (!b)
                    return Result.Error<FolderModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo folder");

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
                return Result.Exception<FolderModel>("Lỗi khi tạo folder", ex);
            }
        }

        public async Task<Result> DeleteFolderAsync(string folderId)
        {
            try
            {
                var entity = await _folderRepo.GetAsync(folderId);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                // Xoá tất cả link folder-file trước
                var folderFiles = await _folderFileRepo.FindAsync(ff => ff.FolderId == folderId);
                if (folderFiles.Any())
                    await _folderFileRepo.BulkDeleteAsync(folderFiles);

                var b = await _folderRepo.DeleteAsync(entity);
                if (!b)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá folder");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi xoá folder", ex);
            }
        }

        public async Task<Result<List<FolderFileItemModel>>> GetFolderFilesAsync(string folderId)
        {
            try
            {
                var folder = await _folderRepo.GetAsync(folderId);
                if (folder == null)
                    return Result.Error<List<FolderFileItemModel>>(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

                var folderFiles = await _folderFileRepo.FindAsync(ff => ff.FolderId == folderId);
                var fileIds = folderFiles.Select(ff => ff.FileId).ToList();

                if (!fileIds.Any())
                    return Result.Ok(new List<FolderFileItemModel>());

                var files = await _fileRepo.FindAsync(f => fileIds.Contains(f.Id));

                var models = files.Select(f => new FolderFileItemModel
                {
                    FileId = f.Id,
                    FileName = f.FileName,
                    FileUrl = $"/FileStorage/GetFile?fileId={f.Id}"
                }).ToList();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                return Result.Exception<List<FolderFileItemModel>>("Lỗi khi lấy danh sách file trong folder", ex);
            }
        }

        public async Task<Result> AddFileToFolderAsync(string folderId, string fileId)
        {
            try
            {
                // Kiểm tra folder tồn tại
                var folder = await _folderRepo.GetAsync(folderId);
                if (folder == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Folder không tồn tại");

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
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể thêm file vào folder");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi thêm file vào folder", ex);
            }
        }

        public async Task<Result> RemoveFileFromFolderAsync(string folderId, string fileId)
        {
            try
            {
                var entity = await _folderFileRepo.FirstOrDefaultAsync(
                    ff => ff.FolderId == folderId && ff.FileId == fileId);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "File không tồn tại trong folder");

                var b = await _folderFileRepo.DeleteAsync(entity);
                if (!b)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá file khỏi folder");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception("Lỗi khi xoá file khỏi folder", ex);
            }
        }
    }
}
