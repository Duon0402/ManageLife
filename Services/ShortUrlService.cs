using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.ShortUrl;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class ShortUrlService : ServiceBase<ShortUrlService>, IShortUrlService
    {
        private readonly IShortUrlRepository _shortUrlRepo;
        private readonly IShortUrlClickRepository _shortUrlClickRepo;
        private readonly ISequentialCodeGenerator _codeGenerator;

        public ShortUrlService(
            IAppLogger<ShortUrlService> logger,
            IUserContext userContext,
            IShortUrlRepository shortUrlRepo,
            IShortUrlClickRepository shortUrlClickRepo,
            ISequentialCodeGenerator codeGenerator) : base(logger, userContext)
        {
            _shortUrlRepo = shortUrlRepo;
            _shortUrlClickRepo = shortUrlClickRepo;
            _codeGenerator = codeGenerator;
        }

        public async Task<Result<List<ShortUrlModel>>> GetListAsync(CancellationToken ct)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var models = await _shortUrlRepo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedTime)
                    .Select(x => new ShortUrlModel
                    {
                        Id = x.Id,
                        Code = x.Code,
                        OriginalUrl = x.OriginalUrl,
                        Title = x.Title,
                        ClickCount = x.ClickCount,
                        ExpireAt = x.ExpireAt,
                        CreatedTime = x.CreatedTime
                    })
                    .ToListAsync(ct);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy danh sách short URL";
                _logger.Error(ex, msg);
                return Result.Exception<List<ShortUrlModel>>(msg, ex);
            }
        }

        public async Task<Result<ShortUrlModel>> GetByCodeAsync(GetShortUrlByCodeRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<ShortUrlModel>(Result.DATA_INVALID.Code, err);

                var entity = await _shortUrlRepo.FirstOrDefaultAsync(
                    x => x.Code == request.Code && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error<ShortUrlModel>(Result.DATA_NOT_EXISTED.Code, "Short URL không tồn tại");

                return Result.Ok(new ShortUrlModel
                {
                    Id = entity.Id,
                    Code = entity.Code,
                    OriginalUrl = entity.OriginalUrl,
                    Title = entity.Title,
                    ClickCount = entity.ClickCount,
                    ExpireAt = entity.ExpireAt,
                    CreatedTime = entity.CreatedTime
                });
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi lấy short URL";
                _logger.Error(ex, msg);
                return Result.Exception<ShortUrlModel>(msg, ex);
            }
        }

        public async Task<Result> CreateAsync(CreateShortUrlRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var code = await _codeGenerator.NextAsync(CodeSequenceCategory.ShortUrl, ct);

                var entity = new ShortUrlEntity
                {
                    Code = code,
                    OriginalUrl = request.OriginalUrl.Trim(),
                    Title = request.Title?.Trim(),
                    ExpireAt = request.ExpireAt,
                    OwnerId = _userContext.GetUserId()
                };

                var inserted = await _shortUrlRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo short URL");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi tạo short URL";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> RecordClickAsync(RecordShortUrlClickRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _shortUrlRepo.FirstOrDefaultAsync(
                    x => x.Code == request.Code && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Short URL không tồn tại");

                var click = new ShortUrlClickEntity
                {
                    ShortUrlId = entity.Id,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    Referer = request.Referer
                };

                await _shortUrlClickRepo.InsertAsync(click, ct);

                entity.ClickCount++;
                await _shortUrlRepo.UpdateAsync(entity, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi ghi click";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteShortUrlRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _shortUrlRepo.FirstOrDefaultAsync(
                    x => x.Id == request.Id && !x.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Short URL không tồn tại");

                var deleted = await _shortUrlRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa short URL");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = "Có lỗi xảy ra khi xóa short URL";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
