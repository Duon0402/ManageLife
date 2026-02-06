using LinqKit;
using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly ITodoListRepository _repo;

        public TodoListService(ITodoListRepository repo)
        {
            _repo = repo;
        }

        public async Task<Result> CreateToDoList(CreateToDoListRequest request)
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

                bool isDuplicate = await IsDuplicateNameAsync(request.Name);
                if (isDuplicate)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<TodoListEntity>();
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

        public async Task<Result> DeleteToDoList(DeleteToDoListRequest request)
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

        public async Task<Result<List<TodoListModel>>> GetListTodoLists()
        {
            string msg;
            try
            {
                var entities = await _repo.FindAsync(x => x.IsDeleted == false);
                var models = entities.MapTo<List<TodoListModel>>();
                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<TodoListModel>>(msg, ex);
            }
        }

        public async Task<Result<TodoListModel>> GetTodoListById(GetTodoListByIdRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(request.Id);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<TodoListModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<TodoListModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<TodoListModel>(msg, ex);
            }
        }

        public async Task<Result> UpdateToDoList(UpdateToDoListRequest request)
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

                bool isDuplicate = await IsDuplicateNameAsync(request.Name, entity.Id);
                if (isDuplicate)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
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
