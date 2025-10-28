using LinqKit;
using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class TodoTaskService : ServiceBase, ITodoTaskService
    {
        private readonly TodoTaskRepository _repo;

        public TodoTaskService(AppDbContext context) : base(context)
        {
            _repo = new TodoTaskRepository(context);
        }

        public async Task<Result> CreateTodoTask(CreateTodoTaskRequest request)
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

                var entity = request.MapTo<TodoTaskEntity>();

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

        public async Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request)
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

        public async Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<List<TodoTaskModel>>(Result.DATA_INVALID.Code, msg);
                }

                var predicate = PredicateBuilder.New<TodoTaskEntity>(x => x.IsDeleted == false);
                if (request.TodoListId.IsNotEmpty())
                {
                    predicate.And(x => x.TodoListId == request.TodoListId);
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
                var entities = await _repo.FindAsync(predicate);
                var models = entities.MapToList<TodoTaskModel>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<TodoTaskModel>>(msg, ex);
            }
        }

        public async Task<Result<TodoTaskModel>> GetTodoTaskById(GetTodoTaskByIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<TodoTaskModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id);
                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<TodoTaskModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }
                var model = entity.MapTo<TodoTaskModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<TodoTaskModel>(msg, ex);
            }
        }

        public async Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request)
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

                var entity = await _repo.GetAsync(x => x.Id == request.Id);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }
    }
}
