using AutoMapper;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class LanguageService : ServiceBase<LanguageService>, ILanguageService
    {
        private readonly ILanguageRepository _repo;
        private readonly ICacheService _cache;

        public LanguageService(IMapper mapper, ILanguageRepository repo, ICacheService cache, IAppLogger<LanguageService> logger, IUserContext userContext) : base(logger, userContext, mapper)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<Result<ChangeLanguageResult>> ChangeLanguageAsync(ChangeLanguageRequest request, string currentLanguage, CancellationToken ct = default)
        {
            try
            {
                var result = request.MapTo<ChangeLanguageResult>();

                if (currentLanguage.IsNotEmpty() &&
                    string.Equals(currentLanguage, request.LanguageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Ok(result);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Code == request.LanguageCode && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<ChangeLanguageResult>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<ChangeLanguageResult>(msg, ex);
            }
        }

        public async Task<Result> CreateLanguageAsync(CreateLanguageRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existing = await _repo.FirstOrDefaultAsync(x => x.Code == request.Code && x.IsDeleted == false, ct);
                if (existing != null)
                {
                    var msg = TranslationKey.Common.Message.DataExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<LanguageEntity>();

                var inserted = await _repo.InsertAsync(entity, ct);
                if (!inserted)
                {
                    var msg = TranslationKey.Common.Message.CreateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await _cache.RemoveAsync(CacheSettings.Languages());

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteLanguageAsync(DeleteLanguageRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request == null)
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var deleted = await _repo.DeleteAsync(entity, ct);

                if (!deleted)
                {
                    var msg = TranslationKey.Common.Message.DeleteError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                await _cache.RemoveAsync(CacheSettings.Languages());

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<LanguageModel>> GetLanguageByCodeAsync(GetLanguageByCodeRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request == null)
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<LanguageModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Code == request.LanguageCode && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<LanguageModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<LanguageModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }

        public async Task<Result<LanguageModel>> GetLanguageByIdAsync(GetLanguageByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request == null)
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<LanguageModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<LanguageModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<LanguageModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }

        public async Task<Result<List<LanguageModel>>> GetListLanguagesAsync(CancellationToken ct = default)
        {
            try
            {
                var cacheItem = CacheSettings.Languages();
                var cached = await _cache.TryGetValueAsync<List<LanguageModel>>(cacheItem);
                if (cached != null)
                    return Result.Ok(cached);

                var entities = await _repo.FindAsync(x => x.IsDeleted == false, ct);
                var models = entities.IsNotEmpty()
                    ? entities.MapToList<LanguageModel>()
                    : new List<LanguageModel>();

                await _cache.SetAsync(models, cacheItem);

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<LanguageModel>>(msg, ex);
            }
        }

        public async Task<Result> UpdateLanguageAsync(UpdateLanguageRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request == null)
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var duplicate = await _repo.FirstOrDefaultAsync(x => x.Code == request.Code && x.Id != request.Id && x.IsDeleted == false, ct);

                if (duplicate != null)
                {
                    var msg = TranslationKey.Common.Message.DataExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                request.MapTo(entity);

                var updated = await _repo.UpdateAsync(entity, ct);

                if (!updated)
                {
                    var msg = TranslationKey.Common.Message.UpdateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                await _cache.RemoveAsync(CacheSettings.Languages());

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }
    }
}
