using LinqKit;
using ManageLife.Commons;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class TodoReminderService : ITodoReminderService
    {
        private readonly ITodoTaskRepository _taskRepo;
        private readonly IUserTelegramConnectionService _telegramConnectionService;
        private readonly ITelegramService _telegramService;
        private readonly IAppLogger<TodoReminderService> _logger;

        public TodoReminderService(
            ITodoTaskRepository taskRepo,
            IUserTelegramConnectionService telegramConnectionService,
            ITelegramService telegramService,
            IAppLogger<TodoReminderService> logger)
        {
            _taskRepo = taskRepo;
            _telegramConnectionService = telegramConnectionService;
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task<Result> ProcessPendingRemindersAsync(CancellationToken ct = default)
        {
            try
            {
                var now = DateTimeHelper.UtcNow();
                var windowEnd = now.AddMinutes(1);

                var predicate = PredicateBuilder.New<TodoTaskEntity>(x =>
                    x.IsDeleted == false &&
                    x.IsReminderSent == false &&
                    x.ReminderAt != null &&
                    x.ReminderAt <= windowEnd &&
                    x.ReminderAt >= now.AddMinutes(-2) &&
                    x.Status != TodoStatus.Completed &&
                    x.Status != TodoStatus.Cancelled
                );

                var tasks = await _taskRepo.FindAsync(predicate, ct);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                    return Result.Ok();

                foreach (var task in taskList)
                {
                    var connectionResult = await _telegramConnectionService
                        .GetUserTelegramConnectionByUserIdAsync(
                            new GetUserTelegramConnectionByUserIdRequest { UserId = task.CreatedUser }, ct);

                    if (!connectionResult.IsOk() || connectionResult.Data == null)
                    {
                        task.IsReminderSent = true;
                        await _taskRepo.UpdateAsync(task, ct);
                        continue;
                    }

                    var message = BuildReminderMessage(task);
                    await _telegramService.SendMessageToChatAsync(connectionResult.Data.ChatId, message, ct);

                    task.IsReminderSent = true;
                    await _taskRepo.UpdateAsync(task, ct);
                }

                _logger.Info("Đã xử lý {count} nhắc nhở công việc", taskList.Count);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xử lý nhắc nhở công việc");
                return Result.Exception("Lỗi khi xử lý nhắc nhở", ex);
            }
        }

        public async Task<Result> SendDailySummaryAsync(CancellationToken ct = default)
        {
            try
            {
                var today = DateTimeHelper.VNTime().Date;
                var tomorrow = today.AddDays(1);

                var predicate = PredicateBuilder.New<TodoTaskEntity>(x =>
                    x.IsDeleted == false &&
                    x.DueDate >= today &&
                    x.DueDate < tomorrow
                );

                var tasks = await _taskRepo.FindAsync(predicate, ct);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                    return Result.Ok();

                var grouped = taskList.GroupBy(x => x.CreatedUser);

                foreach (var group in grouped)
                {
                    var userId = group.Key;
                    var userTasks = group.ToList();

                    var connectionResult = await _telegramConnectionService
                        .GetUserTelegramConnectionByUserIdAsync(
                            new GetUserTelegramConnectionByUserIdRequest { UserId = userId }, ct);

                    if (!connectionResult.IsOk() || connectionResult.Data == null)
                        continue;

                    var message = BuildDailySummaryMessage(userTasks);
                    await _telegramService.SendMessageToChatAsync(connectionResult.Data.ChatId, message, ct);
                }

                _logger.Info("Đã gửi tóm tắt ngày cho {count} người dùng", grouped.Count());
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi tóm tắt ngày");
                return Result.Exception("Lỗi khi gửi tóm tắt ngày", ex);
            }
        }

        private static string BuildReminderMessage(TodoTaskEntity task)
        {
            var dueText = task.DueDate.HasValue
                ? task.DueDate.Value.ToLocalTime().ToString("HH:mm dd/MM/yyyy")
                : "Không có hạn";

            var priorityText = task.Priority switch
            {
                TodoPriority.High => "🔴 Cao",
                TodoPriority.Medium => "🟡 Trung bình",
                TodoPriority.Low => "🟢 Thấp",
                _ => ""
            };

            var descLine = task.Description.IsNotEmpty() ? $"\n📝 {task.Description}" : "";
            return $"🔔 *Nhắc việc*\n\n📌 {task.Title}{descLine}\n⏰ Hạn: {dueText}\n📊 Ưu tiên: {priorityText}";
        }

        private static string BuildDailySummaryMessage(List<TodoTaskEntity> tasks)
        {
            var total = tasks.Count;
            var completed = tasks.Count(x => x.Status == TodoStatus.Completed);
            var pending = tasks.Count(x => x.Status == TodoStatus.Pending);
            var inProgress = tasks.Count(x => x.Status == TodoStatus.InProgress);

            var lines = new System.Text.StringBuilder();
            lines.AppendLine($"📅 *Kế hoạch hôm nay* — {DateTimeHelper.VNTime():dd/MM/yyyy}");
            lines.AppendLine();
            lines.AppendLine($"📊 Tổng: {total} | ✅ Hoàn thành: {completed} | 🔄 Đang làm: {inProgress} | ⏳ Chờ: {pending}");
            lines.AppendLine();

            foreach (var task in tasks.OrderBy(x => x.Priority).ThenBy(x => x.DueDate))
            {
                var statusIcon = task.Status switch
                {
                    TodoStatus.Completed => "✅",
                    TodoStatus.InProgress => "🔄",
                    TodoStatus.Cancelled => "❌",
                    _ => "⏳"
                };
                var timeText = task.DueDate.HasValue
                    ? $" ({task.DueDate.Value.ToLocalTime():HH:mm})"
                    : "";
                lines.AppendLine($"{statusIcon} {task.Title}{timeText}");
            }

            return lines.ToString().TrimEnd();
        }
    }

}
