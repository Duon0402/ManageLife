using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class ExceptionItemService : ServiceBase<ExceptionItemService>, IExceptionItemService
    {
        private readonly IExceptionItemRepository _repo;

        public ExceptionItemService(IExceptionItemRepository repo, IAppLogger<ExceptionItemService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result> CreateExceptionItemAsync(CreateExceptionItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = request.MapTo<ExceptionItemEntity>();
                var inserted = await _repo.InsertAsync(entity, ct);

                if (!inserted)
                {
                    var msg = TranslationKey.Common.Message.CreateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteExceptionItemAsync(DeleteExceptionItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _repo.GetAsync(request.Id, ct);

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

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<ExceptionItemModel>> GetExceptionItemByIdAsync(GetExceptionItemByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<ExceptionItemModel>(Result.DATA_INVALID.Code, err);

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<ExceptionItemModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<ExceptionItemModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<ExceptionItemModel>(msg, ex);
            }
        }

        public async Task<Result<List<ExceptionItemModel>>> GetListExceptionItemsAsync(GetListExceptionItemsRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<List<ExceptionItemModel>>(Result.DATA_INVALID.Code, err);

                var predicate = PredicateBuilder.New<ExceptionItemEntity>(x => x.IsDeleted == false);

                if (request.Type.IsNotEmpty())
                {
                    predicate = predicate.And(x => x.Type == request.Type);
                }

                var entities = await _repo.FindAsync(predicate, ct);

                var models = entities.MapToList<ExceptionItemModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<ExceptionItemModel>>(msg, ex);
            }
        }

        public async Task<Result> UpdateExceptionItemAsync(UpdateExceptionItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                request.MapTo(entity);

                var updated = await _repo.UpdateAsync(entity, ct);

                if (!updated)
                {
                    var msg = TranslationKey.Common.Message.UpdateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
    }
}
