using LinqKit;
using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class ExceptionItemService : IExceptionItemService
    {
        private readonly IExceptionItemRepository _repo;

        public ExceptionItemService(IExceptionItemRepository repo)
        {
            _repo = repo;
        }

        public async Task<Result> CreateExceptionItemAsync(CreateExceptionItemRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = request.MapTo<ExceptionItemEntity>();
                var b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.CreateError;
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteExceptionItemAsync(DeleteExceptionItemRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var b = await _repo.DeleteAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.DeleteError;
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<ExceptionItemModel>> GetExceptionItemByIdAsync(GetExceptionItemByIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<ExceptionItemModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<ExceptionItemModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<ExceptionItemModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<ExceptionItemModel>(msg, ex);
            }
        }

        public async Task<Result<List<ExceptionItemModel>>> GetListExceptionItemsAsync(GetListExceptionItemsRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<List<ExceptionItemModel>>(Result.DATA_INVALID.Code, msg);
                }

                var predicate = PredicateBuilder.New<ExceptionItemEntity>(x => x.IsDeleted == false);

                if (request.Type.IsNotEmpty())
                {
                    predicate = predicate.And(x => x.Type == request.Type);
                }

                var entities = await _repo.FindAsync(predicate);

                var models = entities.MapToList<ExceptionItemModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<ExceptionItemModel>>(msg, ex);
            }
        }

        public async Task<Result> UpdateExceptionItemAsync(UpdateExceptionItemRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                request.MapTo(entity);

                var b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.UpdateError;
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }
    }
}
