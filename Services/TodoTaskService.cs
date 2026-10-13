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
        // Đồng bộ lấy chồng thêm trước mốc (giây): bù lệch thời điểm ghi giữa các request
        private const int SyncOverlapSeconds = 10;
        private const string CompletedLockedMessage = "Việc đã xong nên không sửa được, hãy bỏ đánh dấu xong trước";
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

        public async Task<Result> CreateAsync(SaveTodoTaskRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                // App tạo khi mất mạng rồi gửi lại (cùng id): đã tạo rồi thì trả bản đã có, không tạo trùng
                if (ClientId(request.Id) is { } clientId)
                {
                    var existing = await _taskRepo.Query(true).FirstOrDefaultAsync(x => x.Id == clientId, ct);
                    if (existing != null)
                    {
                        return existing.OwnerId == userId && !existing.IsDeleted
                            ? Result.Ok()
                            : Result.Error(Result.DATA_EXISTED.Code, "Công việc đã bị xoá");
                    }
                }

                var (error, listId) = await ValidateTaskRequestAsync(request, userId, ct);
                if (error != null) return Result.Error(error.Code, error.Message);

                var task = new TodoTaskEntity
                {
                    Id = ClientId(request.Id) ?? IdHelper.NewId(),
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
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo công việc");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi tạo công việc");
                return Result.Exception("Có lỗi xảy ra khi tạo công việc", ex);
            }
        }

        public async Task<Result> UpdateAsync(string id, SaveTodoTaskRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);
                if (task.CompletedAt != null) return Result.Error(Result.DATA_INVALID.Code, CompletedLockedMessage);

                var (error, listId) = await ValidateTaskRequestAsync(request, userId, ct);
                if (error != null) return Result.Error(error.Code, error.Message);

                // Chuyển sang list khác thì đặt task xuống cuối list mới
                if (task.ListId != listId)
                    task.SortOrder = await NextTaskSortOrderAsync(userId, listId, ct);
                ApplyRequest(task, request, listId);

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
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Công việc vừa thay đổi ở nơi khác, hãy mở lại rồi sửa");
                }

                await _taskRepo.UpdateAsync(task, ct);
                await _uow.CommitAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi cập nhật công việc");
                return Result.Exception("Có lỗi xảy ra khi cập nhật công việc", ex);
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

        public async Task<Result> SetCompletedAsync(string id, bool completed, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(id, userId, ct);
                if (task == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                // Gọi lại complete trên task đã xong thì giữ nguyên thời điểm hoàn thành cũ
                var completedAt = completed ? task.CompletedAt ?? DateTimeHelper.UtcNow() : (DateTime?)null;

                if (completed && task.CompletedAt == null && task.RepeatFrequency != null)
                {
                    await _uow.BeginTransactionAsync(ct);
                    //NOTE: "Giành" task bằng UPDATE có điều kiện: bấm 2 lần / 2 máy cùng tick thì chỉ 1 request tạo lần kế tiếp
                    var claimed = await _taskRepo.Query()
                        .Where(x => x.Id == task.Id && x.CompletedAt == null && !x.IsDeleted)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.CompletedAt, completedAt), ct);
                    if (claimed == 1)
                    {
                        //NOTE: Đọc lại bản mới nhất (dòng đang bị khoá bởi lệnh trên) — có thể vừa có lần lưu khác commit trước.
                        // Không dùng lại bản cũ: ghi bản cũ (IsDeleted = false) sẽ khôi phục việc vừa bị xoá ở nơi khác
                        var latest = await FindOwnedTaskAsync(id, userId, ct);
                        if (latest == null)
                        {
                            await _uow.RollbackAsync(ct);
                            return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);
                        }
                        task = latest;
                        await SpawnNextOccurrenceAsync(task, ct);
                        task.CompletedAt = completedAt;
                        await _taskRepo.UpdateAsync(task, ct);
                        await _uow.CommitAsync(ct);
                    }
                    else
                    {
                        // Request khác đã hoàn thành (và tạo lần kế tiếp) trước: không ghi đè
                        await _uow.RollbackAsync(ct);
                    }
                }
                else
                {
                    task.CompletedAt = completedAt;
                    if (!await _taskRepo.UpdateAsync(task, ct))
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật công việc");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi đổi trạng thái công việc");
                return Result.Exception("Có lỗi xảy ra khi đổi trạng thái công việc", ex);
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

        public async Task<Result> AddChecklistItemAsync(string taskId, SaveChecklistItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var task = await FindOwnedTaskAsync(taskId, userId, ct);
                if (task == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, NotFoundMessage);

                // Gửi lại cùng id (đồng bộ từ app): đã thêm rồi thì trả mục đã có
                var clientId = ClientId(request.Id);
                if (clientId != null)
                {
                    var existing = await _checklistRepo.Query(true).FirstOrDefaultAsync(x => x.Id == clientId, ct);
                    if (existing != null)
                    {
                        return existing.TaskId == taskId
                            ? Result.Ok()
                            : Result.Error(Result.DATA_EXISTED.Code, "Mục checklist đã tồn tại");
                    }
                }

                if (task.CompletedAt != null) return Result.Error(Result.DATA_INVALID.Code, CompletedLockedMessage);

                var maxSort = await _checklistRepo.Query(true)
                    .Where(x => x.TaskId == taskId)
                    .MaxAsync(x => (int?)x.SortOrder, ct);

                var item = new TodoChecklistItemEntity
                {
                    Id = clientId ?? IdHelper.NewId(),
                    TaskId = taskId,
                    Title = request.Title.Trim(),
                    IsDone = request.IsDone,
                    SortOrder = (maxSort ?? -1) + 1
                };

                //NOTE: Thêm mục + đánh dấu việc vừa đổi trong 1 transaction: lỗi giữa chừng mà mục đã lưu nhưng việc chưa
                // đổi UpdatedTime thì lần đồng bộ sau của app không lấy lại checklist
                await _uow.BeginTransactionAsync(ct);
                if (!await _checklistRepo.InsertAsync(item, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể thêm mục checklist");
                }
                await TouchTaskAsync(taskId, ct);
                await _uow.CommitAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi thêm mục checklist");
                return Result.Exception("Có lỗi xảy ra khi thêm mục checklist", ex);
            }
        }

        public async Task<Result> UpdateChecklistItemAsync(string itemId, SaveChecklistItemRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var item = await FindOwnedChecklistItemAsync(itemId, userId, ct);
                if (item == null) return Result.Error(Result.DATA_NOT_EXISTED.Code, ChecklistNotFoundMessage);
                if (await IsTaskCompletedAsync(item.TaskId, ct)) return Result.Error(Result.DATA_INVALID.Code, CompletedLockedMessage);

                item.Title = request.Title.Trim();
                item.IsDone = request.IsDone;

                await _uow.BeginTransactionAsync(ct);
                if (!await _checklistRepo.UpdateAsync(item, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật mục checklist");
                }
                await TouchTaskAsync(item.TaskId, ct);
                await _uow.CommitAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi cập nhật mục checklist");
                return Result.Exception("Có lỗi xảy ra khi cập nhật mục checklist", ex);
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
                if (await IsTaskCompletedAsync(item.TaskId, ct)) return Result.Error(Result.DATA_INVALID.Code, CompletedLockedMessage);

                await _uow.BeginTransactionAsync(ct);
                if (!await _checklistRepo.DeleteAsync(item, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá mục checklist");
                }
                await TouchTaskAsync(item.TaskId, ct);
                await _uow.CommitAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi xoá mục checklist");
                return Result.Exception("Có lỗi xảy ra khi xoá mục checklist", ex);
            }
        }

        /// <summary>
        /// Thay đổi từ mốc `since` cho app lưu trên máy (không có mốc: toàn bộ). Lấy chồng thêm vài giây trước mốc để
        /// không lỡ thay đổi ghi cùng lúc với lần đồng bộ trước (nhận trùng thì app chỉ ghi đè lại).
        /// </summary>
        public async Task<Result<TodoSyncModel>> GetChangesAsync(DateTime? since, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<TodoSyncModel>(Result.DATA_INVALID.Code, NoUserMessage);

                var serverTime = DateTimeHelper.UtcNow();
                var from = since.HasValue ? ToUtc(since)!.Value.AddSeconds(-SyncOverlapSeconds) : (DateTime?)null;

                var lists = await _listRepo.Query(true)
                    .Where(x => x.OwnerId == userId)
                    .Where(x => from == null
                        ? !x.IsDeleted
                        : x.CreatedTime >= from || x.UpdatedTime >= from || x.DeletedTime >= from)
                    .ToListAsync(ct);
                var tasks = await _taskRepo.Query(true)
                    .Where(x => x.OwnerId == userId)
                    .Where(x => from == null
                        ? !x.IsDeleted
                        : x.CreatedTime >= from || x.UpdatedTime >= from || x.DeletedTime >= from)
                    .ToListAsync(ct);

                var liveTasks = tasks.Where(x => !x.IsDeleted).ToList();
                var liveIds = liveTasks.Select(x => x.Id).ToList();
                var items = liveIds.IsEmpty()
                    ? new List<TodoChecklistItemEntity>()
                    : await _checklistRepo.Query(true)
                        .Where(x => liveIds.Contains(x.TaskId))
                        .OrderBy(x => x.SortOrder)
                        .ToListAsync(ct);
                var itemsByTask = items.GroupBy(x => x.TaskId).ToDictionary(x => x.Key, x => x.ToList());
                // Tên/màu danh sách của công việc (kể cả danh sách không đổi trong lần này)
                var listIds = liveTasks.Where(x => x.ListId != null).Select(x => x.ListId!).Distinct().ToList();
                var listsById = await _listRepo.Query(true).Where(x => listIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

                var model = new TodoSyncModel
                {
                    ServerTime = serverTime,
                    Full = from == null,
                    Lists = lists.Where(x => !x.IsDeleted).MapToList<TodoListEntity, TodoListModel>(),
                    DeletedListIds = lists.Where(x => x.IsDeleted).Select(x => x.Id).ToList(),
                    DeletedTaskIds = tasks.Where(x => x.IsDeleted).Select(x => x.Id).ToList(),
                    Tasks = liveTasks.Select(task =>
                    {
                        var taskItems = itemsByTask.GetValueOrDefault(task.Id) ?? new List<TodoChecklistItemEntity>();
                        var detail = task.MapTo<TodoTaskDetailModel>();
                        detail.Checklist = taskItems.MapToList<TodoChecklistItemEntity, TodoChecklistItemModel>();
                        FillComputed(detail, task, listsById, (taskItems.Count, taskItems.Count(x => x.IsDone)));
                        return detail;
                    }).ToList(),
                };
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đồng bộ công việc");
                return Result.Exception<TodoSyncModel>("Có lỗi xảy ra khi đồng bộ công việc", ex);
            }
        }

        /// <summary>Id app gửi lên (GUID) chuẩn hoá về dạng chữ thường; không hợp lệ/không có thì null.</summary>
        private static string? ClientId(string? id) => Guid.TryParse(id, out var guid) ? guid.ToString() : null;

        /// <summary>Đánh dấu công việc vừa đổi (checklist đổi) để lần đồng bộ sau của app lấy lại checklist.</summary>
        private Task<int> TouchTaskAsync(string taskId, CancellationToken ct) =>
            _taskRepo.Query()
                .Where(x => x.Id == taskId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedTime, DateTimeHelper.UtcNow()), ct);

        #region Helpers

        private static DateOnly TodayVn() => DateOnly.FromDateTime(DateTimeHelper.VNTime());

        private Task<TodoTaskEntity?> FindOwnedTaskAsync(string id, string userId, CancellationToken ct) =>
            _taskRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);

        // Checklist không có OwnerId: quyền sở hữu suy ra qua task cha
        //NOTE: Việc đã xong bị khoá sửa (cả checklist); bỏ đánh dấu xong để sửa
        private Task<bool> IsTaskCompletedAsync(string taskId, CancellationToken ct) =>
            _taskRepo.Query(true).AnyAsync(x => x.Id == taskId && x.CompletedAt != null, ct);

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
            var previousDueDate = task.DueDate;
            task.Title = request.Title.Trim();
            task.Note = request.Note.IsEmpty() ? null : request.Note!.Trim();
            task.ListId = listId;
            task.Priority = (TodoPriority)request.Priority;
            task.DueDate = request.DueDate;
            task.DueTime = request.DueTime;
            task.ReminderAt = ToUtc(request.ReminderAt);
            task.ReminderAlarm = task.ReminderAt != null && request.ReminderAlarm;

            var frequency = (TodoRepeatFrequency?)request.RepeatFrequency;
            // Ngày gốc (lặp tháng/năm): giữ nguyên nếu không đổi ngày hạn (hạn hiện tại có thể đã bị kẹp, vd 28/2 của chuỗi ngày 31)
            var keepMonthDay = task.RepeatMonthDay != null && previousDueDate == request.DueDate;
            task.RepeatMonthDay = frequency is TodoRepeatFrequency.Monthly or TodoRepeatFrequency.Yearly
                ? keepMonthDay ? task.RepeatMonthDay : (byte?)request.DueDate!.Value.Day
                : null;
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
        private async Task SpawnNextOccurrenceAsync(TodoTaskEntity task, CancellationToken ct)
        {
            if (task.RepeatFrequency is not { } frequency || task.DueDate is not { } due) return;

            var nextDue = TodoRepeatHelper.NextDueDate(
                due, frequency, task.RepeatInterval ?? 1, task.RepeatWeekdays, task.RepeatUntil, TodayVn(), task.RepeatMonthDay);

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
                    ReminderAlarm = task.ReminderAlarm,
                    RepeatFrequency = task.RepeatFrequency,
                    RepeatInterval = task.RepeatInterval,
                    RepeatWeekdays = task.RepeatWeekdays,
                    RepeatUntil = task.RepeatUntil,
                    RepeatMonthDay = task.RepeatMonthDay,
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
        }

        private static void ClearRepeat(TodoTaskEntity task)
        {
            task.RepeatFrequency = null;
            task.RepeatInterval = null;
            task.RepeatWeekdays = null;
            task.RepeatUntil = null;
            task.RepeatMonthDay = null;
        }

        private async Task<int> NextTaskSortOrderAsync(string userId, string? listId, CancellationToken ct)
        {
            var maxSort = await _taskRepo.Query(true)
                .Where(x => x.OwnerId == userId && !x.IsDeleted && x.ListId == listId)
                .MaxAsync(x => (int?)x.SortOrder, ct);
            return (maxSort ?? -1) + 1;
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
