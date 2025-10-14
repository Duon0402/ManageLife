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
    public class TodoListService : ServiceBase, ITodoListService
    {
        private readonly TodoListRepository _repo;

        public TodoListService(AppDbContext context) : base(context)
        {
            _repo = new TodoListRepository(context);
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

        public Task<Result> DeleteToDoList(DeleteToDoListRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<TodoListModel>>> GetListTodoLists(GetListTodoListsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<TodoListModel>> GetTodoListById(GetTodoListByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateToDoList(UpdateToDoListRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
