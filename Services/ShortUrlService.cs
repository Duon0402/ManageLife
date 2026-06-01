using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.ShortUrl;

namespace ManageLife.Services
{
    public class ShortUrlService : ServiceBase<ShortUrlService>, IShortUrlService
    {
        private readonly IShortUrlRepository _shortUrlRepo;
        private readonly ISequentialCodeGenerator _codeGenerator;

        public ShortUrlService(
            IAppLogger<ShortUrlService> logger,
            IUserContext userContext,
            IShortUrlRepository shortUrlRepo,
            ISequentialCodeGenerator codeGenerator) : base(logger, userContext)
        {
            _shortUrlRepo = shortUrlRepo;
            _codeGenerator = codeGenerator;
        }

        public async Task<Result> CreateAsync(CreateShortUrlRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var shortUrl = request.MapTo<ShortUrlEntity>();

                var code = await _codeGenerator.NextAsync(CodeSequenceCategory.ShortUrl, ct);

                if (code.IsEmpty())
                {
                    _logger.Debug("Failed to generate code for short URL.");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Failed to create short URL.");
                }

                shortUrl.Code = code;

                var isInserted = await _shortUrlRepo.InsertAsync(shortUrl, ct);

                if (!isInserted)
                {
                    _logger.Debug("Failed to insert short URL.");
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Failed to create short URL.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while creating short URL.");
                return Result.Exception("An unexpected error occurred while creating short URL.", ex);
            }
        }

        public async Task<Result> DeleteAsync(DeleteShortUrlRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var shortUrl = await _shortUrlRepo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (shortUrl == null)
                {
                    _logger.Debug($"Short URL with ID {request.Id} not found.");
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Short URL not found.");
                }

                var isDeleted = await _shortUrlRepo.DeleteAsync(shortUrl, ct);

                if (!isDeleted)
                {
                    _logger.Debug($"Failed to delete short URL with ID {request.Id}.");
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Failed to delete short URL.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while deleting short URL.");
                return Result.Exception("An unexpected error occurred while deleting short URL.", ex);
            }
        }

        public async Task<Result<ShortUrlModel>> GetByCodeAsync(GetShortUrlByCodeRequest request, CancellationToken ct)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<ShortUrlModel>(Result.DATA_INVALID.Code, err);

                var shortUrl = await _shortUrlRepo.FirstOrDefaultAsync(x => x.Code == request.Code && x.IsDeleted == false, ct);

                if (shortUrl == null)
                {
                    _logger.Debug($"Short URL with code {request.Code} not found.");
                    return Result.Error<ShortUrlModel>(Result.DATA_NOT_EXISTED.Code, "Short URL not found.");
                }

                var result = shortUrl.MapTo<ShortUrlModel>();

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while getting short URL.");
                return Result.Exception<ShortUrlModel>("An unexpected error occurred while getting short URL.", ex);
            }
        }

        public Task<Result<List<ShortUrlModel>>> GetListAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result> RecordClickAsync(RecordShortUrlClickRequest request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
