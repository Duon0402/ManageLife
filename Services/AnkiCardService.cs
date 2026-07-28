using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class AnkiCardService : ServiceBase<AnkiCardService>, IAnkiCardService
    {
        private const string ClozePlaceholder = "___";

        private readonly IAnkiCardRepository _repo;

        public AnkiCardService(IAppLogger<AnkiCardService> logger, IUserContext userContext, IAnkiCardRepository repo)
            : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result<List<AnkiCardModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<AnkiCardModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var models = await _repo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .OrderByDescending(x => x.RecordedDate)
                    .Select(x => new AnkiCardModel
                    {
                        Id = x.Id,
                        CardType = x.CardType,
                        FieldFront = x.FieldFront,
                        FieldBack = x.FieldBack,
                        FieldExtra = x.FieldExtra,
                        SourceNote = x.SourceNote,
                        RecordedDate = x.RecordedDate
                    })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách thẻ Anki");
                return Result.Exception<List<AnkiCardModel>>("Có lỗi xảy ra khi lấy danh sách thẻ Anki", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateAnkiCardRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var clozeErr = ValidateCloze(request.CardType, request.FieldFront);
                if (clozeErr.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, clozeErr);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = new AnkiCardEntity
                {
                    OwnerId = userId,
                    CardType = request.CardType,
                    FieldFront = request.FieldFront.Trim(),
                    FieldBack = request.FieldBack.Trim(),
                    FieldExtra = request.FieldExtra?.Trim(),
                    SourceNote = request.SourceNote?.Trim(),
                    RecordedDate = DateTime.UtcNow
                };

                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo thẻ Anki");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo thẻ Anki");
                return Result.Exception("Có lỗi xảy ra khi tạo thẻ Anki", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateAnkiCardRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var clozeErr = ValidateCloze(request.CardType, request.FieldFront);
                if (clozeErr.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, clozeErr);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.Id == request.Id && x.OwnerId == userId && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Thẻ Anki không tồn tại");

                entity.CardType = request.CardType;
                entity.FieldFront = request.FieldFront.Trim();
                entity.FieldBack = request.FieldBack.Trim();
                entity.FieldExtra = request.FieldExtra?.Trim();
                entity.SourceNote = request.SourceNote?.Trim();

                var updated = await _repo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật thẻ Anki");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi cập nhật thẻ Anki");
                return Result.Exception("Có lỗi xảy ra khi cập nhật thẻ Anki", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                if (id.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Id không hợp lệ");

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Thẻ Anki không tồn tại");

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa thẻ Anki");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xóa thẻ Anki");
                return Result.Exception("Có lỗi xảy ra khi xóa thẻ Anki", ex);
            }
        }

        public async Task<Result<List<AnkiCardEntity>>> GetAllForExportAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<AnkiCardEntity>>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entities = await _repo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .OrderBy(x => x.RecordedDate)
                    .ToListAsync(ct);

                return Result.Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy toàn bộ thẻ Anki để xuất file");
                return Result.Exception<List<AnkiCardEntity>>("Có lỗi xảy ra khi lấy dữ liệu xuất file", ex);
            }
        }

        private static string? ValidateCloze(AnkiCardType cardType, string fieldFront)
        {
            if (cardType == AnkiCardType.Cloze && !fieldFront.Contains(ClozePlaceholder))
                return $"Nội dung Cloze phải chứa \"{ClozePlaceholder}\" để đánh dấu chỗ trống";

            return null;
        }
    }
}
