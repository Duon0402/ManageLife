using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using System.Text.Json;

namespace ManageLife.Services
{
    /// <summary>
    /// Đồng bộ công việc với app (app lưu trên máy, làm được khi mất mạng): 1 request gồm mọi thao tác chưa gửi, áp dụng
    /// lần lượt đúng thứ tự rồi trả thay đổi về. Thao tác bị từ chối (dữ liệu không hợp lệ, đã bị xoá...) thì bỏ qua và
    /// trả toàn bộ dữ liệu để máy về đúng như server; lỗi server thì dừng, app gửi lại phần còn lại lần sau.
    /// </summary>
    public class TodoSyncService : ServiceBase<TodoSyncService>, ITodoSyncService
    {
        private const int MaxOps = 100;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly HashSet<string> DeleteTypes = new() { "deleteList", "deleteTask", "deleteChecklist" };

        private readonly ITodoListService _listService;
        private readonly ITodoTaskService _taskService;
        private readonly IUnitOfWork _uow;

        public TodoSyncService(
            IAppLogger<TodoSyncService> logger,
            IUserContext userContext,
            ITodoListService listService,
            ITodoTaskService taskService,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _listService = listService;
            _taskService = taskService;
            _uow = uow;
        }

        public async Task<Result<TodoSyncModel>> SyncAsync(TodoSyncRequest request, CancellationToken ct = default)
        {
            try
            {
                //NOTE: JSON gửi "ops": null thì đè giá trị mặc định
                request.Ops ??= new List<TodoSyncOperation>();
                if (request.Ops.Count > MaxOps)
                    return Result.Error<TodoSyncModel>(Result.DATA_INVALID.Code, $"Mỗi lần đồng bộ tối đa {MaxOps} thao tác");

                var rejected = new List<string>();
                for (var i = 0; i < request.Ops.Count; i++)
                {
                    var op = request.Ops[i];
                    var rs = await ApplyAsync(op, ct);
                    //NOTE: Thao tác lỗi có thể để lại entity đã sửa trong DbContext: bỏ hết để thao tác sau không lưu nhầm
                    _uow.Context.ChangeTracker.Clear();

                    if (rs.IsException())
                        return Result.Ok(new TodoSyncModel { Processed = i, Rejected = rejected });

                    // Gửi lại lệnh xoá (lần trước server đã xoá nhưng app không nhận được phản hồi) coi như xong
                    if (!rs.IsOk() && !(DeleteTypes.Contains(op.Type) && rs.Code == Result.DATA_NOT_EXISTED.Code))
                        rejected.Add(rs.Message);
                }

                var changes = await _taskService.GetChangesAsync(rejected.Count == 0 ? request.Since : null, ct);
                if (!changes.IsOk())
                    return Result.Ok(new TodoSyncModel { Processed = request.Ops.Count, Rejected = rejected });

                var model = changes.Data!;
                model.Processed = request.Ops.Count;
                model.Complete = true;
                model.Rejected = rejected;
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đồng bộ công việc");
                return Result.Exception<TodoSyncModel>("Có lỗi xảy ra khi đồng bộ công việc", ex);
            }
        }

        private Task<Result> ApplyAsync(TodoSyncOperation op, CancellationToken ct)
        {
            try
            {
                return op.Type switch
                {
                    "createList" => _listService.CreateAsync(Body<SaveTodoListRequest>(op, true), ct),
                    "updateList" => _listService.UpdateAsync(RequireId(op), Body<SaveTodoListRequest>(op), ct),
                    "deleteList" => _listService.DeleteAsync(RequireId(op), ct),
                    "reorderLists" => _listService.ReorderAsync(new ReorderRequest { Ids = op.Ids ?? new() }, ct),
                    "createTask" => _taskService.CreateAsync(Body<SaveTodoTaskRequest>(op, true), ct),
                    "updateTask" => _taskService.UpdateAsync(RequireId(op), Body<SaveTodoTaskRequest>(op), ct),
                    "deleteTask" => _taskService.DeleteAsync(RequireId(op), ct),
                    "completeTask" => _taskService.SetCompletedAsync(RequireId(op), op.Completed, op.SpawnNext ?? true, ct),
                    "reorderTasks" => _taskService.ReorderAsync(new ReorderRequest { Ids = op.Ids ?? new() }, ct),
                    "addChecklist" => _taskService.AddChecklistItemAsync(
                        op.TaskId ?? throw new InvalidOperationException("Thiếu công việc của mục checklist"),
                        Body<SaveChecklistItemRequest>(op, true), ct),
                    "updateChecklist" => _taskService.UpdateChecklistItemAsync(RequireId(op), Body<SaveChecklistItemRequest>(op), ct),
                    "deleteChecklist" => _taskService.DeleteChecklistItemAsync(RequireId(op), ct),
                    _ => Task.FromResult(Result.Error(Result.DATA_INVALID.Code, $"Thao tác không hỗ trợ: {op.Type}"))
                };
            }
            catch (Exception ex)
            {
                // Chỉ lỗi đọc dữ liệu thao tác (service tự bắt lỗi của nó): dữ liệu gửi lên sai, từ chối thao tác này
                // thay vì dừng cả lần đồng bộ
                _logger.Debug("Thao tác đồng bộ không hợp lệ: {Error}", ex.Message);
                return Task.FromResult(Result.Error(Result.DATA_INVALID.Code, ex is InvalidOperationException ? ex.Message : "Dữ liệu không hợp lệ"));
            }
        }

        private static string RequireId(TodoSyncOperation op) =>
            op.Id.IsNotEmpty() ? op.Id! : throw new InvalidOperationException("Thiếu id");

        /// <summary>
        /// Đọc dữ liệu tạo/sửa; tạo mới thì id nằm ở thao tác (GUID app sinh), gán vào request. Bắt buộc GUID: server tự
        /// sinh id khác thì gửi lại (mất phản hồi) sẽ tạo trùng, và id trên máy không khớp server.
        /// </summary>
        private static T Body<T>(TodoSyncOperation op, bool withId = false) where T : class
        {
            if (withId && !Guid.TryParse(op.Id, out _)) throw new InvalidOperationException("Id không hợp lệ");
            var body = op.Body is { ValueKind: JsonValueKind.Object } element
                ? element.Deserialize<T>(JsonOptions)
                : null;
            if (body == null) throw new InvalidOperationException("Thiếu dữ liệu");
            if (withId)
            {
                switch (body)
                {
                    case SaveTodoListRequest list: list.Id = op.Id; break;
                    case SaveTodoTaskRequest task: task.Id = op.Id; break;
                    case SaveChecklistItemRequest item: item.Id = op.Id; break;
                }
            }
            return body;
        }
    }
}
