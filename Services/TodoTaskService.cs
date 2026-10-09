using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class TodoTaskService : ServiceBase<TodoTaskService>, ITodoTaskService
    {
        private const string NotFoundMessage = "Không tìm thấy công việc";
        private const string ChecklistNotFoundMessage = "Không tìm thấy mục checklist";
        private const string ListNotFoundMessage = "Không tìm thấy danh sách";
        private const string NoUserMessage = "Không xác định được người dùng";
        private const int UpcomingDays = 7;
        private const int ReminderSyncDays = 30;
        private static readonly DateOnly MaxDate = new(2100, 12, 31);

        private readonly ITodoTaskRepository _taskRepo;
        private readonly ITodoListRepository _listRepo;
        private readonly ITodoChecklistItemRepository _checklistRepo;
        private readonly IUnitOfWork _uow;

        public TodoTaskService(
            IAppLogger<TodoTaskService> logger,
            IUserContext userContext,
            ITodoTaskRepository taskRepo,
            ITodoListRepository listRepo,
            ITodoChecklistItemRepository checklistRepo,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _taskRepo = taskRepo;
            _listRepo = listRepo;
            _checklistRepo = checklistRepo;
            _uow = uow;
        }

        public async Task<Result<PageList<TodoTaskModel>>> GetListAsync(GetTodoTasksRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<PageList<TodoTaskModel>>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<PageList<TodoTaskModel>>(Result.DATA_INVALID.Code, NoUserMessage);

                string? listId = null;
                if (request.ListId.IsNotEmpty())
                {
                    if (!await IsOwnedListAsync(request.ListId, userId, ct))
                        return Result.Error<PageList<TodoTaskModel>>(Result.DATA_NOT_EXISTED.Code, ListNotFoundMessage);
                    listId = request.ListId;
                }
                else if (request.View == "list")
                {
                    return Result.Error<PageList<TodoTaskModel>>(Result.DATA_INVALID.Code, "Cần chọn danh sách");
                }

                var today = TodayVn();
                var upcomingEnd = today.AddDays(UpcomingDays);
                var nowUtc = DateTimeHelper.UtcNow();
                var reminderEnd = nowUtc.AddDays(ReminderSyncDays);
                var query = _taskRepo.Query(true).Where(x => x.OwnerId == userId && !x.IsDeleted);

                IQueryable<TodoTaskEntity> view = request.View switch
                {
                    // Gồm cả việc quá hạn
                    "today" => query
                        .Where(x => x.CompletedAt == null && x.DueDate != null && x.DueDate <= today)
                        .OrderBy(x => x.DueDate).ThenBy(x => x.DueTime == null).ThenBy(x => x.DueTime)
                        .ThenByDescending(x => x.Priority).ThenBy(x => x.SortOrder)
                        .ThenBy(x => x.CreatedTime).ThenBy(x => x.Id),
                    "upcoming" => query
                        .Where(x => x.CompletedAt == null && x.DueDate > today && x.DueDate <= upcomingEnd)
                        .OrderBy(x => x.DueDate).ThenBy(x => x.DueTime == null).ThenBy(x => x.DueTime)
                        .ThenBy(x => x.SortOrder).ThenBy(x => x.CreatedTime).ThenBy(x => x.Id),
                    "inbox" => query
                        .Where(x => x.CompletedAt == null && x.ListId == null)
                        .OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedTime).ThenBy(x => x.Id),
                    "list" => query
                        .Where(x => x.CompletedAt == null && x.ListId == listId)
                        .OrderBy(x => x.SortOrder).ThenBy(x => x.CreatedTime).ThenBy(x => x.Id),
                    // Nhắc sắp tới: app dùng để đặt lại thông báo trên máy
                    "reminders" => query
                        .Where(x => x.CompletedAt == null && x.ReminderAt != null && x.ReminderAt > nowUtc && x.ReminderAt <= reminderEnd)
                        .OrderBy(x => x.ReminderAt).ThenBy(x => x.Id),
                    _ => query
                        .Where(x => x.CompletedAt != null && (listId == null || x.ListId == listId))
                        .OrderByDescending(x => x.CompletedAt).ThenByDescending(x => x.Id)
                };

                var page = await view.ToPageListAsync(request.PageIndex, request.PageSize, ct);
                var models = await ToModelsAsync(page.Items, ct);

                return Result.Ok(new PageList<TodoTaskModel>(models, page.TotalItems, page.PageIndex, page.PageSize));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách công việc");
                return Result.Exception<PageList<TodoTaskModel>>("Có lỗi xảy ra khi lấy danh sách công việc", ex);
            }
        }

        public async Task<Result<TodoTaskDetailModel>> GetByIdAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoTaskDetailModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error<TodoTaskDetailModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                return Result.Ok(await ToDetailModelAsync(task, ct));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy chi tiết công việc");
                return Result.Exception<TodoTaskDetailModel>("Có lỗi xảy ra khi lấy chi tiết công việc", ex);
            }
        }

        public async Task<Result<TodoTaskDetailModel>> CreateAsync(SaveTodoTaskRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoTaskDetailModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var (error, listId) = await ValidateTaskRequestAsync(request, userId, ct);
                if (error != null) return Result.Error<TodoTaskDetailModel>(error.Code, error.Message);

                var task = new TodoTaskEntity
                {
                    Id = IdHelper.NewId(),
                    OwnerId = userId,
                    SortOrder = await NextTaskSortOrderAsync(userId, listId, ct)
                };
                ApplyRequest(task, request, listId);

                var checklistTitles = (request.Checklist ?? new List<string>())
                    .Where(x => x.IsNotEmpty())
                    .Select(x => x.Trim())
                    .ToList();

                await _uow.BeginTransactionAsync(ct);

                var created = await _taskRepo.InsertAsync(task, ct);
                if (created && checklistTitles.IsNotEmpty())
                {
                    created = await _checklistRepo.BulkInsertAsync(checklistTitles.Select((title, index) => new TodoChecklistItemEntity
                    {
                        Id = IdHelper.NewId(),
                        TaskId = task.Id,
                        Title = title,
                        SortOrder = index
                    }), ct);
                }

                if (!created)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error<TodoTaskDetailModel>(Result.DATA_NOT_CREATE.Code, "Không thể tạo công việc");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok(await ToDetailModelAsync(task, ct));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi tạo công việc");
                return Result.Exception<TodoTaskDetailModel>("Có lỗi xảy ra khi tạo công việc", ex);
            }
        }

        public async Task<Result<TodoTaskDetailModel>> UpdateAsync(string id, SaveTodoTaskRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoTaskDetailModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error<TodoTaskDetailModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                var (error, listId) = await ValidateTaskRequestAsync(request, userId, ct);
                if (error != null) return Result.Error<TodoTaskDetailModel>(error.Code, error.Message);

                // Chuyển sang list khác thì đặt task xuống cuối list mới
                if (task.ListId != listId)
                    task.SortOrder = await NextTaskSortOrderAsync(userId, listId, ct);
                ApplyRequest(task, request, listId);
                // Việc đã xong không lặp (chuỗi lặp đã chuyển sang lần kế tiếp)
                if (task.CompletedAt != null) ClearRepeat(task);

                await _uow.BeginTransactionAsync(ct);
                //NOTE: Khoá dòng và kiểm tra trạng thái xong chưa đổi từ lúc đọc: lưu trùng lúc tick xong ở nơi khác
                // thì không ghi đè (tránh mở lại việc đã xong + khôi phục chuỗi lặp → tạo trùng lần kế tiếp)
                var readCompletedAt = task.CompletedAt;
                var unchanged = await _taskRepo.Query()
                    .Where(x => x.Id == task.Id && x.CompletedAt == readCompletedAt)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedTime, DateTimeHelper.UtcNow()), ct);
                if (unchanged != 1)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error<TodoTaskDetailModel>(Result.DATA_NOT_UPDATE.Code, "Công việc vừa thay đổi ở nơi khác, hãy mở lại rồi sửa");
                }

                await _taskRepo.UpdateAsync(task, ct);
                await _uow.CommitAsync(ct);

                return Result.Ok(await ToDetailModelAsync(task, ct));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi cập nhật công việc");
                return Result.Exception<TodoTaskDetailModel>("Có lỗi xảy ra khi cập nhật công việc", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                if (!await _taskRepo.DeleteAsync(task, ct))
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá công việc");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá công việc");
                return Result.Exception("Có lỗi xảy ra khi xoá công việc", ex);
            }
        }

        public async Task<Result<TodoTaskModel>> SetCompletedAsync(string id, bool completed, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoTaskModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error<TodoTaskModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                DateOnly? nextDue = null;
                // Gọi lại complete trên task đã xong thì giữ nguyên thời điểm hoàn thành cũ
                var completedAt = completed ? task.CompletedAt ?? DateTimeHelper.UtcNow() : (DateTime?)null;

                if (completed && task.CompletedAt == null && task.RepeatFrequency != null)
                {
                    await _uow.BeginTransactionAsync(ct);
                    //NOTE: "Giành" task bằng UPDATE có điều kiện: bấm 2 lần / 2 máy cùng tick thì chỉ 1 request tạo lần kế tiếp
                    var claimed = await _taskRepo.Query()
                        .Where(x => x.Id == task.Id && x.CompletedAt == null)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.CompletedAt, completedAt), ct);
                    if (claimed == 1)
                    {
                        //NOTE: Đọc lại bản mới nhất (dòng đang bị khoá bởi lệnh trên) — có thể vừa có lần lưu khác commit trước
                        task = await FindOwnedTaskAsync(id, userId, ct) ?? task;
                        nextDue = await SpawnNextOccurrenceAsync(task, ct);
                        task.CompletedAt = completedAt;
                        await _taskRepo.UpdateAsync(task, ct);
                        await _uow.CommitAsync(ct);
                    }
                    else
                    {
                        // Request khác đã hoàn thành (và tạo lần kế tiếp) trước: không ghi đè, trả trạng thái hiện tại
                        await _uow.RollbackAsync(ct);
                        task = await FindOwnedTaskAsync(id, userId, ct);
                        if (task == null) return Result.Error<TodoTaskModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);
                    }
                }
                else
                {
                    task.CompletedAt = completedAt;
                    if (!await _taskRepo.UpdateAsync(task, ct))
                        return Result.Error<TodoTaskModel>(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật công việc");
                }

                var model = (await ToModelsAsync(new List<TodoTaskEntity> { task }, ct))[0];
                model.NextOccurrenceDueDate = nextDue;
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi đổi trạng thái công việc");
                return Result.Exception<TodoTaskModel>("Có lỗi xảy ra khi đổi trạng thái công việc", ex);
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
                var tasks = await _taskRepo.Query()
                    .Where(x => x.OwnerId == userId && !x.IsDeleted && ids.Contains(x.Id))
                    .ToListAsync(ct);

                // Id lạ hoặc của người khác bị bỏ qua
                foreach (var task in tasks)
                    task.SortOrder = ids.IndexOf(task.Id);

                if (tasks.IsNotEmpty() && !await _taskRepo.BulkUpdateAsync(tasks, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể sắp xếp công việc");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi sắp xếp công việc");
                return Result.Exception("Có lỗi xảy ra khi sắp xếp công việc", ex);
            }
        }

        public async Task<Result<TodoChecklistItemModel>> AddChecklistItemAsync(string taskId, SaveChecklistItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoChecklistItemModel>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoChecklistItemModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(taskId, userId, ct);
                if (task == null) return Result.Error<TodoChecklistItemModel>(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                var maxSort = await _checklistRepo.Query(true)
                    .Where(x => x.TaskId == taskId)
                    .MaxAsync(x => (int?)x.SortOrder, ct);

                var item = new TodoChecklistItemEntity
                {
                    TaskId = taskId,
                    Title = request.Title.Trim(),
                    IsDone = request.IsDone,
                    SortOrder = (maxSort ?? -1) + 1
                };

                if (!await _checklistRepo.InsertAsync(item, ct))
                    return Result.Error<TodoChecklistItemModel>(Result.DATA_NOT_CREATE.Code, "Không thể thêm mục checklist");

                return Result.Ok(item.MapTo<TodoChecklistItemModel>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi thêm mục checklist");
                return Result.Exception<TodoChecklistItemModel>("Có lỗi xảy ra khi thêm mục checklist", ex);
            }
        }

        public async Task<Result<TodoChecklistItemModel>> UpdateChecklistItemAsync(string itemId, SaveChecklistItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<TodoChecklistItemModel>(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoChecklistItemModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var item = await FindOwnedChecklistItemAsync(itemId, userId, ct);
                if (item == null) return Result.Error<TodoChecklistItemModel>(Result.DATA_NOT_EXISTED.Code, ChecklistNotFoundMessage);

                item.Title = request.Title.Trim();
                item.IsDone = request.IsDone;

                if (!await _checklistRepo.UpdateAsync(item, ct))
                    return Result.Error<TodoChecklistItemModel>(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật mục checklist");

                return Result.Ok(item.MapTo<TodoChecklistItemModel>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi cập nhật mục checklist");
                return Result.Exception<TodoChecklistItemModel>("Có lỗi xảy ra khi cập nhật mục checklist", ex);
            }
        }

        public async Task<Result> DeleteChecklistItemAsync(string itemId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var item = await FindOwnedChecklistItemAsync(itemId, userId, ct);
                if (item == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, ChecklistNotFoundMessage);

                if (!await _checklistRepo.DeleteAsync(item, ct))
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá mục checklist");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá mục checklist");
                return Result.Exception("Có lỗi xảy ra khi xoá mục checklist", ex);
            }
        }

        public async Task<Result<TodoSummaryModel>> GetSummaryAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoSummaryModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var today = TodayVn();
                var startOfTodayUtc = today.ToDateTime(TimeOnly.MinValue).ToUtcFromVnTime();
                var startOfTomorrowUtc = today.AddDays(1).ToDateTime(TimeOnly.MinValue).ToUtcFromVnTime();
                var owned = _taskRepo.Query(true).Where(x => x.OwnerId == userId && !x.IsDeleted);

                return Result.Ok(new TodoSummaryModel
                {
                    Today = await owned.CountAsync(x => x.CompletedAt == null && x.DueDate == today, ct),
                    Overdue = await owned.CountAsync(x => x.CompletedAt == null && x.DueDate < today, ct),
                    CompletedToday = await owned.CountAsync(x => x.CompletedAt >= startOfTodayUtc && x.CompletedAt < startOfTomorrowUtc, ct)
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy tổng quan công việc");
                return Result.Exception<TodoSummaryModel>("Có lỗi xảy ra khi lấy tổng quan công việc", ex);
            }
        }

        #region Helpers

        private static DateOnly TodayVn() => DateOnly.FromDateTime(DateTimeHelper.VNTime());

        private Task<TodoTaskEntity?> FindOwnedTaskAsync(string id, string userId, CancellationToken ct) =>
            _taskRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);

        // Checklist không có OwnerId: quyền sở hữu suy ra qua task cha
        private async Task<TodoChecklistItemEntity?> FindOwnedChecklistItemAsync(string itemId, string userId, CancellationToken ct)
        {
            var item = await _checklistRepo.FirstOrDefaultAsync(x => x.Id == itemId, ct);
            if (item == null) return null;
            return await FindOwnedTaskAsync(item.TaskId, userId, ct) == null ? null : item;
        }

        private Task<bool> IsOwnedListAsync(string listId, string userId, CancellationToken ct) =>
            _listRepo.Query(true).AnyAsync(x => x.Id == listId && x.OwnerId == userId && !x.IsDeleted, ct);

        private async Task<(Result? Error, string? ListId)> ValidateTaskRequestAsync(SaveTodoTaskRequest request, string userId, CancellationToken ct)
        {
            var err = Validate(request);
            if (err.IsNotEmpty()) return (Result.Error(Result.DATA_INVALID.Code, err), null);

            if (request.DueTime.HasValue && !request.DueDate.HasValue)
                return (Result.Error(Result.DATA_INVALID.Code, "Cần chọn ngày trước khi chọn giờ"), null);

            if (request.RepeatFrequency.HasValue && !request.DueDate.HasValue)
                return (Result.Error(Result.DATA_INVALID.Code, "Việc lặp lại cần có ngày hạn"), null);

            //NOTE: Giới hạn khoảng ngày hợp lý (tránh tràn DateOnly khi tính lần lặp kế tiếp)
            if (request.DueDate > MaxDate || request.RepeatUntil > MaxDate)
                return (Result.Error(Result.DATA_INVALID.Code, "Ngày không được sau năm 2100"), null);

            if (request.RepeatFrequency.HasValue && request.RepeatUntil < request.DueDate)
                return (Result.Error(Result.DATA_INVALID.Code, "Ngày kết thúc lặp phải từ ngày hạn trở đi"), null);

            if (request.Checklist != null && request.Checklist.Any(x => x.IsNotEmpty() && x.Trim().Length > 500))
                return (Result.Error(Result.DATA_INVALID.Code, "Mỗi mục checklist tối đa 500 ký tự"), null);

            if (request.ListId.IsEmpty()) return (null, null);

            if (!await IsOwnedListAsync(request.ListId, userId, ct))
                return (Result.Error(Result.DATA_NOT_EXISTED.Code, ListNotFoundMessage), null);

            return (null, request.ListId);
        }

        // Hợp đồng: ReminderAt là UTC. Chuỗi không có múi giờ (Kind Unspecified) coi là UTC thay vì giờ máy chủ.
        private static DateTime? ToUtc(DateTime? value) => value switch
        {
            null => null,
            { Kind: DateTimeKind.Unspecified } v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            var v => v.Value.ToUniversalTime()
        };

        private static void ApplyRequest(TodoTaskEntity task, SaveTodoTaskRequest request, string? listId)
        {
            task.Title = request.Title.Trim();
            task.Note = request.Note.IsEmpty() ? null : request.Note!.Trim();
            task.ListId = listId;
            task.Priority = (TodoPriority)request.Priority;
            task.DueDate = request.DueDate;
            task.DueTime = request.DueTime;
            task.ReminderAt = ToUtc(request.ReminderAt);

            var frequency = (TodoRepeatFrequency?)request.RepeatFrequency;
            task.RepeatFrequency = frequency;
            task.RepeatInterval = frequency.HasValue ? request.RepeatInterval ?? 1 : null;
            // Hằng tuần mà không chọn thứ thì lặp đúng thứ của ngày hạn
            task.RepeatWeekdays = frequency == TodoRepeatFrequency.Weekly
                ? (byte?)request.RepeatWeekdays ?? TodoRepeatHelper.WeekdayBit(request.DueDate!.Value.DayOfWeek)
                : null;
            task.RepeatUntil = frequency.HasValue ? request.RepeatUntil : null;
        }

        /// <summary>
        /// Việc lặp vừa hoàn thành: tạo lần kế tiếp (bản sao, hạn mới, lời nhắc dời theo, checklist chưa tick)
        /// và chuyển chuỗi lặp sang lần mới — bỏ tick rồi tick lại việc cũ không sinh trùng.
        /// </summary>
        /// <returns>Ngày hạn lần kế tiếp đã tạo; null nếu hết chuỗi.</returns>
        private async Task<DateOnly?> SpawnNextOccurrenceAsync(TodoTaskEntity task, CancellationToken ct)
        {
            if (task.RepeatFrequency is not { } frequency || task.DueDate is not { } due) return null;

            var nextDue = TodoRepeatHelper.NextDueDate(
                due, frequency, task.RepeatInterval ?? 1, task.RepeatWeekdays, task.RepeatUntil, TodayVn());

            if (nextDue.HasValue)
            {
                var next = new TodoTaskEntity
                {
                    Id = IdHelper.NewId(),
                    OwnerId = task.OwnerId,
                    ListId = task.ListId,
                    Title = task.Title,
                    Note = task.Note,
                    Priority = task.Priority,
                    DueDate = nextDue,
                    DueTime = task.DueTime,
                    ReminderAt = task.ReminderAt?.AddDays(nextDue.Value.DayNumber - due.DayNumber),
                    RepeatFrequency = task.RepeatFrequency,
                    RepeatInterval = task.RepeatInterval,
                    RepeatWeekdays = task.RepeatWeekdays,
                    RepeatUntil = task.RepeatUntil,
                    SortOrder = await NextTaskSortOrderAsync(task.OwnerId, task.ListId, ct)
                };
                await _taskRepo.InsertAsync(next, ct);

                var checklist = await _checklistRepo.Query(true)
                    .Where(x => x.TaskId == task.Id)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync(ct);
                if (checklist.Count != 0)
                {
                    await _checklistRepo.BulkInsertAsync(checklist.Select(x => new TodoChecklistItemEntity
                    {
                        Id = IdHelper.NewId(),
                        TaskId = next.Id,
                        Title = x.Title,
                        SortOrder = x.SortOrder
                    }), ct);
                }
            }

            ClearRepeat(task);
            return nextDue;
        }

        private static void ClearRepeat(TodoTaskEntity task)
        {
            task.RepeatFrequency = null;
            task.RepeatInterval = null;
            task.RepeatWeekdays = null;
            task.RepeatUntil = null;
        }

        private async Task<int> NextTaskSortOrderAsync(string userId, string? listId, CancellationToken ct)
        {
            var maxSort = await _taskRepo.Query(true)
                .Where(x => x.OwnerId == userId && !x.IsDeleted && x.ListId == listId)
                .MaxAsync(x => (int?)x.SortOrder, ct);
            return (maxSort ?? -1) + 1;
        }

        private async Task<List<TodoTaskModel>> ToModelsAsync(List<TodoTaskEntity> tasks, CancellationToken ct)
        {
            if (tasks.IsEmpty()) return new List<TodoTaskModel>();

            var taskIds = tasks.Select(x => x.Id).ToList();
            var listIds = tasks.Where(x => x.ListId != null).Select(x => x.ListId!).Distinct().ToList();

            // 1 query cho checklist + 1 query cho list, tránh N+1
            var checklist = await _checklistRepo.Query(true)
                .Where(x => taskIds.Contains(x.TaskId))
                .GroupBy(x => x.TaskId)
                .Select(g => new { TaskId = g.Key, Total = g.Count(), Done = g.Count(x => x.IsDone) })
                .ToDictionaryAsync(x => x.TaskId, ct);

            var lists = listIds.IsEmpty()
                ? new Dictionary<string, TodoListEntity>()
                : await _listRepo.Query(true).Where(x => listIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

            return tasks.Select(task =>
            {
                var model = task.MapTo<TodoTaskModel>();
                FillComputed(model, task, lists, checklist.TryGetValue(task.Id, out var c) ? (c.Total, c.Done) : (0, 0));
                return model;
            }).ToList();
        }

        private async Task<TodoTaskDetailModel> ToDetailModelAsync(TodoTaskEntity task, CancellationToken ct)
        {
            var items = await _checklistRepo.Query(true)
                .Where(x => x.TaskId == task.Id)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(ct);

            var lists = task.ListId == null
                ? new Dictionary<string, TodoListEntity>()
                : await _listRepo.Query(true).Where(x => x.Id == task.ListId).ToDictionaryAsync(x => x.Id, ct);

            var model = task.MapTo<TodoTaskDetailModel>();
            model.Checklist = items.MapToList<TodoChecklistItemEntity, TodoChecklistItemModel>();
            FillComputed(model, task, lists, (items.Count, items.Count(x => x.IsDone)));
            return model;
        }

        /// <summary>Gán các field không có trong entity (tên/màu list, số mục checklist).</summary>
        private static void FillComputed(TodoTaskModel model, TodoTaskEntity task, Dictionary<string, TodoListEntity> lists, (int Total, int Done) checklist)
        {
            var list = task.ListId != null && lists.TryGetValue(task.ListId, out var l) ? l : null;
            model.ListName = list?.Name;
            model.ListColor = list?.Color;
            model.ChecklistTotal = checklist.Total;
            model.ChecklistDone = checklist.Done;
        }

        #endregion
    }
}
