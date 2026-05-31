using AutoMapper;
using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class TodoTaskService : ServiceBase<TodoTaskService>, ITodoTaskService
    {
        private readonly ITodoTaskRepository _repo;

        public TodoTaskService(IMapper mapper, ITodoTaskRepository repo, IAppLogger<TodoTaskService> logger, IUserContext userContext) : base(logger, userContext, mapper)
        {
            _repo = repo;
        }

        public async Task<Result> CreateTodoTask(CreateTodoTaskRequest request, CancellationToken ct = default)
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

                var entity = request.MapTo<TodoTaskEntity>();

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

        public async Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request, CancellationToken ct = default)
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

        public async Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<List<TodoTaskModel>>(Result.DATA_INVALID.Code, msg);
                }

                var predicate = PredicateBuilder.New<TodoTaskEntity>(x => x.IsDeleted == false);
                if (request.TodoListId.IsNotEmpty())
                {
                    predicate = predicate.And(x => x.TodoListId == request.TodoListId);
                }
                if (request.FromDate.HasValue && request.FromDate != DateTime.MinValue)
                {
                    var fromDate = request.FromDate.Value.Date;
                    predicate = predicate.And(x => x.CreatedTime >= fromDate);
                }
                if (request.ToDate.HasValue && request.ToDate != DateTime.MinValue)
                {
                    var toDate = request.ToDate.Value.Date.AddDays(1);
                    predicate = predicate.And(x => x.CreatedTime < toDate);
                }
                if (request.Status.HasValue)
                {
                    predicate = predicate.And(x => x.Status == request.Status);
                }
                if (request.Priority.HasValue)
                {
                    predicate = predicate.And(x => x.Priority == request.Priority);
                }
                var entities = await _repo.FindAsync(predicate, ct);
                var models = entities.MapToList<TodoTaskModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<TodoTaskModel>>(msg, ex);
            }
        }

        public async Task<Result<TodoTaskModel>> GetTodoTaskById(GetTodoTaskByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error<TodoTaskModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id, ct);
                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TodoTaskModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }
                var model = entity.MapTo<TodoTaskModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<TodoTaskModel>(msg, ex);
            }
        }

        public async Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request, CancellationToken ct = default)
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

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                throw new NotImplementedException("UpdateTodoTask chưa được implement.");
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
