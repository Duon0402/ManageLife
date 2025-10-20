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
        private readonly TodoTaskRepository _repoTask;
        private readonly TodoListRepository _repoList;

        public TodoTaskService(AppDbContext context) : base(context)
        {
            _repoTask = new TodoTaskRepository(context);
            _repoList = new TodoListRepository(context);
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

                var b = await _repoTask.InsertAsync(entity);
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

        public Task<Result> DeleteTodoTask(DeleteTodoTaskRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<TodoTaskModel>>> GetListTodoTasks(GetListTodoTasksRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<TodoListModel>> GetTodoTaskById(GetTodoTaskByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateTodoTask(UpdateTodoTaskRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
