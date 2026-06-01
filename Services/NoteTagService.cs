using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class NoteTagService : ServiceBase<NoteTagService>, INoteTagService
    {
        private readonly INoteTagRepository _tagRepo;
        private readonly INoteTagRelationRepository _relationRepo;

        public NoteTagService(
            IAppLogger<NoteTagService> logger,
            IUserContext userContext,
            INoteTagRepository tagRepo,
            INoteTagRelationRepository relationRepo) : base(logger, userContext)
        {
            _tagRepo = tagRepo;
            _relationRepo = relationRepo;
        }

        public async Task<Result<List<NoteTagModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var tags = await _tagRepo.Query(true)
                    .Where(t => t.OwnerId == userId && !t.IsDeleted)
                    .OrderBy(t => t.Name)
                    .Select(t => new NoteTagModel { Id = t.Id, Name = t.Name, Color = t.Color })
                    .ToListAsync(ct);
                return Result.Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách tag thất bại");
                return Result.Exception<List<NoteTagModel>>("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateNoteTagRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                var existed = await _tagRepo.FirstOrDefaultAsync(t => t.OwnerId == userId && t.Name == request.Name.Trim() && !t.IsDeleted, ct);
                if (existed != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Tag '{request.Name}' đã tồn tại");

                var entity = new NoteTagEntity
                {
                    Name = request.Name.Trim(),
                    Color = request.Color,
                    OwnerId = userId
                };
                var inserted = await _tagRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo tag");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Tạo tag thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateNoteTagRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                var entity = await _tagRepo.FirstOrDefaultAsync(t => t.Id == request.Id && t.OwnerId == userId && !t.IsDeleted, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Tag không tồn tại");

                var duplicate = await _tagRepo.FirstOrDefaultAsync(t => t.OwnerId == userId && t.Name == request.Name.Trim() && t.Id != request.Id && !t.IsDeleted, ct);
                if (duplicate != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Tag '{request.Name}' đã tồn tại");

                entity.Name = request.Name.Trim();
                entity.Color = request.Color;
                var updated = await _tagRepo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật tag");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cập nhật tag thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var entity = await _tagRepo.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId && !t.IsDeleted, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Tag không tồn tại");

                // xóa các relation trước
                var relations = await _relationRepo.FindAsync(r => r.TagId == id, ct);
                foreach (var rel in relations)
                    await _relationRepo.DeleteAsync(rel, ct);

                var deleted = await _tagRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa tag");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa tag thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }
    }
}
