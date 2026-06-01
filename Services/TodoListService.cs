using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class TodoListService : ServiceBase<TodoListService>, ITodoListService
    {
        private readonly ITodoListRepository _repo;

        public TodoListService(ITodoListRepository repo, IAppLogger<TodoListService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
        }

        public async Task<Result> CreateToDoList(CreateToDoListRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                bool isDuplicate = await IsDuplicateNameAsync(request.Name);
                if (isDuplicate)
                {
                    var msg = TranslationKey.Common.Message.DataExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<TodoListEntity>();
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

        public async Task<Result> DeleteToDoList(DeleteToDoListRequest request, CancellationToken ct = default)
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

        public async Task<Result<List<TodoListModel>>> GetListTodoLists(CancellationToken ct = default)
        {
            try
            {
                var entities = await _repo.FindAsync(x => x.IsDeleted == false, ct);
                var models = entities.MapTo<List<TodoListModel>>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<TodoListModel>>(msg, ex);
            }
        }

        public async Task<Result<TodoListModel>> GetTodoListById(GetTodoListByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, err);

                var entity = await _repo.GetAsync(request.Id, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TodoListModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<TodoListModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<TodoListModel>(msg, ex);
            }
        }

        public async Task<Result> UpdateToDoList(UpdateToDoListRequest request, CancellationToken ct = default)
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

                bool isDuplicate = await IsDuplicateNameAsync(request.Name, entity.Id);
                if (isDuplicate)
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

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task<bool> IsDuplicateNameAsync(string name, string? id = null)
        {
            name = name.Trim().ToLower();

            var predicate = PredicateBuilder.New<TodoListEntity>(
                x => !x.IsDeleted && x.Name.ToLower() == name
            );

            if (id.IsNotEmpty())
                predicate = predicate.And(x => x.Id != id);

            var entity = await _repo.FirstOrDefaultAsync(predicate);
            return entity != null;
        }
    }
}
