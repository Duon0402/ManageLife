using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class VocabWordService : ServiceBase<VocabWordService>, IVocabWordService
    {
        private readonly IVocabWordRepository _wordRepo;
        private readonly IDictionaryApiService _dictionaryService;

        public VocabWordService(
            IAppLogger<VocabWordService> logger,
            IUserContext userContext,
            IVocabWordRepository wordRepo,
            IDictionaryApiService dictionaryService) : base(logger, userContext)
        {
            _wordRepo = wordRepo;
            _dictionaryService = dictionaryService;
        }

        public async Task<Result<List<VocabWordModel>>> GetListAsync(GetListVocabWordsRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<VocabWordModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var query = _wordRepo.Query(true)
                    .Where(w => w.OwnerId == userId && !w.IsDeleted);

                if (request.SearchKeyword.IsNotEmpty())
                    query = query.Where(w => w.Word.Contains(request.SearchKeyword!)
                                         || w.Definition!.Contains(request.SearchKeyword!));

                var entities = await query.OrderBy(w => w.Word).ToListAsync(ct);
                return Result.Ok(entities.MapToList<VocabWordEntity, VocabWordModel>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách từ vựng thất bại");
                return Result.Exception<List<VocabWordModel>>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result<VocabWordModel>> GetByIdAsync(GetVocabWordByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<VocabWordModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = await _wordRepo.FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.OwnerId == userId && !w.IsDeleted, ct);

                if (entity == null)
                    return Result.Error<VocabWordModel>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy từ vựng.");

                return Result.Ok(entity.MapTo<VocabWordModel>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy từ vựng thất bại");
                return Result.Exception<VocabWordModel>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateVocabWordRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = request.MapTo<VocabWordEntity>();
                entity.OwnerId = userId!;
                entity.DictionarySource = (VocabDictionarySource)request.DictionarySource;

                var inserted = await _wordRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Tạo từ vựng thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Tạo từ vựng thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateVocabWordRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = await _wordRepo.FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.OwnerId == userId && !w.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy từ vựng.");

                request.MapTo<UpdateVocabWordRequest, VocabWordEntity>(entity);

                var updated = await _wordRepo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Cập nhật từ vựng thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cập nhật từ vựng thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteVocabWordRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = await _wordRepo.FirstOrDefaultAsync(
                    w => w.Id == request.Id && w.OwnerId == userId && !w.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy từ vựng.");

                var deleted = await _wordRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Xóa từ vựng thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa từ vựng thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public Task<Result<DictionaryLookupResult>> LookupFromDictionaryAsync(LookupWordRequest request, CancellationToken ct = default)
            => _dictionaryService.LookupAsync(request.Word, ct);
    }
}
