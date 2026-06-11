using LinqKit;
using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class TodoTaskService : ServiceBase<TodoTaskService>, ITodoTaskService
    {
        private readonly ITodoTaskRepository _repo;
        private readonly ITodoListRepository _listRepo;

        public TodoTaskService(ITodoTaskRepository repo, ITodoListRepository listRepo, IAppLogger<TodoTaskService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
            _listRepo = listRepo;
        }

        private async Task PopulateListNamesAsync(List<TodoTaskModel> models, CancellationToken ct)
        {
            var listIds = models.Select(x => x.TodoListId).Distinct().ToList();
            if (listIds.Count == 0) return;

            var lists = await _listRepo.FindAsync(x => listIds.Contains(x.Id) && !x.IsDeleted, ct);
            var listMap = lists.ToDictionary(x => x.Id, x => x.Name);

            foreach (var m in models)
                m.TodoListName = listMap.GetValueOrDefault(m.TodoListId);
        }

        public async Task<Result> CreateAsync(CreateTodoTaskRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

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

        public async Task<Result> DeleteAsync(DeleteTodoTaskRequest request, CancellationToken ct = default)
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

        public async Task<Result<List<TodoTaskModel>>> GetListAsync(GetListTodoTasksRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<List<TodoTaskModel>>(Result.DATA_INVALID.Code, err);

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
                await PopulateListNamesAsync(models, ct);
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<TodoTaskModel>>(msg, ex);
            }
        }

        public async Task<Result<TodoTaskModel>> GetByIdAsync(GetTodoTaskByIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoTaskModel>(Result.DATA_INVALID.Code, err);

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

        public async Task<Result> UpdateAsync(UpdateTodoTaskRequest request, CancellationToken ct = default)
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

                request.MapTo(entity);

                // Reset IsReminderSent nếu thời gian nhắc thay đổi
                if (entity.ReminderAt != request.ReminderAt)
                    entity.IsReminderSent = false;

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

        public async Task<Result> ChangeStatusAsync(ChangeTaskStatusRequest request, CancellationToken ct = default)
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

                entity.Status = request.Status;
                entity.CompletedAt = request.Status == TodoStatus.Completed ? DateTimeHelper.UtcNow() : null;

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

        public async Task<Result<List<TodoTaskModel>>> GetTodayTasksAsync(CancellationToken ct = default)
        {
            try
            {
                var today = DateTimeHelper.VNTime().Date;
                var tomorrow = today.AddDays(1);

                var predicate = PredicateBuilder.New<TodoTaskEntity>(x =>
                    x.IsDeleted == false &&
                    x.Status != TodoStatus.Completed &&
                    x.Status != TodoStatus.Cancelled &&
                    x.CreatedUser == _userContext.GetUserName()
                );
                predicate = predicate.And(x =>
                    (x.DueDate >= today && x.DueDate < tomorrow) ||
                    (x.StartDate <= today && (x.DueDate == null || x.DueDate >= today))
                );

                var entities = await _repo.FindAsync(predicate, ct);
                var models = entities.MapToList<TodoTaskModel>();
                await PopulateListNamesAsync(models, ct);
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<TodoTaskModel>>(msg, ex);
            }
        }
    }
}
