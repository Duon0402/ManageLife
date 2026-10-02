using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class TodoListService : ServiceBase<TodoListService>, ITodoListService
    {
        private const string NotFoundMessage = "Không tìm thấy danh sách";
        private const string NoUserMessage = "Không xác định được người dùng";
        private const string DuplicateMessage = "Tên danh sách đã tồn tại";

        private readonly ITodoListRepository _listRepo;
        private readonly ITodoTaskRepository _taskRepo;
        private readonly IUnitOfWork _uow;

        public TodoListService(
            IAppLogger<TodoListService> logger,
            IUserContext userContext,
            ITodoListRepository listRepo,
            ITodoTaskRepository taskRepo,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _listRepo = listRepo;
            _taskRepo = taskRepo;
            _uow = uow;
        }

        public async Task<Result<List<TodoListModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<List<TodoListModel>>(Result.DATA_INVALID.Code, NoUserMessage);

                var lists = await _listRepo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedTime)
                    .ToListAsync(ct);

                var openCounts = await CountOpenTasksAsync(userId, ct);

                return Result.Ok(lists.Select(x => ToModel(x, openCounts)).ToList());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách todo list");
                return Result.Exception<List<TodoListModel>>("Có lỗi xảy ra khi lấy danh sách", ex);
            }
        }

        public async Task<Result<TodoListModel>> CreateAsync(SaveTodoListRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var name = request.Name.Trim();
                if (await IsDuplicateNameAsync(userId, name, null, ct))
                    return Result.Error<TodoListModel>(Result.DATA_EXISTED.Code, DuplicateMessage);

                var maxSort = await _listRepo.Query(true)
                    .Where(x => x.OwnerId == userId && !x.IsDeleted)
                    .MaxAsync(x => (int?)x.SortOrder, ct);

                var entity = new TodoListEntity
                {
                    OwnerId = userId,
                    Name = name,
                    Color = request.Color,
                    Icon = request.Icon?.Trim(),
                    SortOrder = (maxSort ?? -1) + 1
                };

                if (!await _listRepo.InsertAsync(entity, ct))
                    return Result.Error<TodoListModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo danh sách");

                return Result.Ok(ToModel(entity, new Dictionary<string, int>()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo todo list");
                return Result.Exception<TodoListModel>("Có lỗi xảy ra khi tạo danh sách", ex);
            }
        }

        public async Task<Result<TodoListModel>> UpdateAsync(string id, SaveTodoListRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoListModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var entity = await _listRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);
                if (entity == null) return Result.Error<TodoListModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                var name = request.Name.Trim();
                if (await IsDuplicateNameAsync(userId, name, id, ct))
                    return Result.Error<TodoListModel>(Result.DATA_EXISTED.Code, DuplicateMessage);

                entity.Name = name;
                entity.Color = request.Color;
                entity.Icon = request.Icon?.Trim();

                if (!await _listRepo.UpdateAsync(entity, ct))
                    return Result.Error<TodoListModel>(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật danh sách");

                return Result.Ok(ToModel(entity, await CountOpenTasksAsync(userId, ct)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi cập nhật todo list");
                return Result.Exception<TodoListModel>("Có lỗi xảy ra khi cập nhật danh sách", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var entity = await _listRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);
                if (entity == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                await _uow.BeginTransactionAsync(ct);

                // FK cascade chỉ chạy khi xoá cứng: phải tự soft delete các task của list
                var tasks = await _taskRepo.Query()
                    .Where(x => x.ListId == id && x.OwnerId == userId && !x.IsDeleted)
                    .ToListAsync(ct);

                var deleted = (tasks.IsEmpty() || await _taskRepo.BulkDeleteAsync(tasks, ct))
                    && await _listRepo.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá danh sách");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi xoá todo list");
                return Result.Exception("Có lỗi xảy ra khi xoá danh sách", ex);
            }
        }

        public async Task<Result> ReorderAsync(ReorderRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var ids = request.Ids.Distinct().ToList();
                var lists = await _listRepo.Query()
                    .Where(x => x.OwnerId == userId && !x.IsDeleted && ids.Contains(x.Id))
                    .ToListAsync(ct);

                // Id lạ hoặc của người khác bị bỏ qua
                foreach (var list in lists)
                    list.SortOrder = ids.IndexOf(list.Id);

                if (lists.IsNotEmpty() && !await _listRepo.BulkUpdateAsync(lists, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể sắp xếp danh sách");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi sắp xếp todo list");
                return Result.Exception("Có lỗi xảy ra khi sắp xếp danh sách", ex);
            }
        }

        private Task<bool> IsDuplicateNameAsync(string userId, string name, string? excludeId, CancellationToken ct)
        {
            var lowered = name.ToLower();
            return _listRepo.Query(true).AnyAsync(x =>
                x.OwnerId == userId && !x.IsDeleted && x.Name.ToLower() == lowered && x.Id != excludeId, ct);
        }

        private Task<Dictionary<string, int>> CountOpenTasksAsync(string userId, CancellationToken ct)
        {
            return _taskRepo.Query(true)
                .Where(x => x.OwnerId == userId && !x.IsDeleted && x.CompletedAt == null && x.ListId != null)
                .GroupBy(x => x.ListId!)
                .Select(g => new { ListId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ListId, x => x.Count, ct);
        }

        private static TodoListModel ToModel(TodoListEntity entity, Dictionary<string, int> openCounts)
        {
            var model = entity.MapTo<TodoListModel>();
            model.OpenCount = openCounts.GetValueOrDefault(entity.Id);
            return model;
        }
    }
}
