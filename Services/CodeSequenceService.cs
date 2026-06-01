using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class CodeSequenceService : ServiceBase<CodeSequenceService>, ICodeSequenceService
    {
        private readonly ICodeSequenceRepository _repo;

        public CodeSequenceService(
            ICodeSequenceRepository repo,
            IAppLogger<CodeSequenceService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result<List<CodeSequenceModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var models = await _repo.Query(true)
                    .OrderBy(x => x.Category)
                    .Select(x => new CodeSequenceModel
                    {
                        Id = x.Id,
                        Category = x.Category,
                        Prefix = x.Prefix,
                        Suffix = x.Suffix,
                        NumberLength = x.NumberLength,
                        CurrentSeq = x.CurrentSeq,
                        CreatedTime = x.CreatedTime
                    })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy danh sách code sequence";
                _logger.Error(ex, msg);
                return Result.Exception<List<CodeSequenceModel>>(msg, ex);
            }
        }

        public async Task<Result> CreateAsync(CreateCodeSequenceRequest request, CancellationToken ct = default)
        {
            try
            {
                var existed = await _repo.FirstOrDefaultAsync(x => x.Category == request.Category, ct);
                if (existed != null)
                    return Result.Error(Result.DATA_EXISTED.Code, $"Category '{request.Category}' đã tồn tại");

                var entity = new CodeSequenceEntity
                {
                    Category = request.Category.Trim(),
                    Prefix = request.Prefix.Trim(),
                    Suffix = request.Suffix?.Trim(),
                    NumberLength = request.NumberLength,
                    CurrentSeq = 0
                };

                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo code sequence");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi tạo code sequence";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateCodeSequenceRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Code sequence không tồn tại");

                var categoryTrimmed = request.Category.Trim();
                if (entity.Category != categoryTrimmed)
                {
                    var existed = await _repo.FirstOrDefaultAsync(x => x.Category == categoryTrimmed, ct);
                    if (existed != null)
                        return Result.Error(Result.DATA_EXISTED.Code, $"Category '{categoryTrimmed}' đã tồn tại");
                    entity.Category = categoryTrimmed;
                }

                entity.Prefix = request.Prefix.Trim();
                entity.Suffix = request.Suffix?.Trim();
                entity.NumberLength = request.NumberLength;

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật code sequence");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi cập nhật code sequence";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> ResetAsync(ResetCodeSequenceRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Code sequence không tồn tại");

                entity.CurrentSeq = request.Value;

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể reset code sequence");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi reset code sequence";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteCodeSequenceRequest request, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Code sequence không tồn tại");

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa code sequence");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi xóa code sequence";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
