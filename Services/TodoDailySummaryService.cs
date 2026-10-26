using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace ManageLife.Services
{
    public class TodoDailySummaryService : ServiceBase<TodoDailySummaryService>, ITodoDailySummaryService
    {
        private const int MaxLinesPerGroup = 20;
        private const int MaxTitleLength = 100;
        //NOTE: Telegram giới hạn 4096 ký tự/tin; chừa chỗ cho dòng "… và N việc khác" và nhóm sau
        private const int MaxMessageLength = 3800;

        private readonly ITodoTaskRepository _taskRepo;
        private readonly ITodoListRepository _listRepo;
        private readonly IUserTelegramConnectionRepository _connectionRepo;
        private readonly ITelegramService _telegramService;
        private readonly ISettingContext _settingContext;
        private readonly IFinanceSummaryBuilder _financeSummary;

        public TodoDailySummaryService(
            IAppLogger<TodoDailySummaryService> logger,
            IUserContext userContext,
            ITodoTaskRepository taskRepo,
            ITodoListRepository listRepo,
            IUserTelegramConnectionRepository connectionRepo,
            ITelegramService telegramService,
            ISettingContext settingContext,
            IFinanceSummaryBuilder financeSummary) : base(logger, userContext)
        {
            _taskRepo = taskRepo;
            _listRepo = listRepo;
            _connectionRepo = connectionRepo;
            _telegramService = telegramService;
            _settingContext = settingContext;
            _financeSummary = financeSummary;
        }

        public async Task<Result> SendDailySummaryAsync(CancellationToken ct = default)
        {
            try
            {
                //NOTE: Tắt tính năng ở admin thì bỏ phần đó; tắt cả Công việc lẫn Tài chính thì không gửi
                var todoEnabled = await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableTodo, true);
                var financeEnabled = await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableFinance, true);
                if (!todoEnabled && !financeEnabled) return Result.Ok();

                var connections = await _connectionRepo.Query(true).Where(x => !x.IsDeleted).ToListAsync(ct);
                if (connections.Count == 0) return Result.Ok();

                var today = DateOnly.FromDateTime(DateTimeHelper.VNTime());
                var userIds = connections.Select(x => x.UserId).Distinct().ToList();

                var tasks = !todoEnabled ? new List<SummaryTask>() : await _taskRepo.Query(true)
                    // Tới hạn/quá hạn, hoặc việc nhiều ngày đang trong khoảng bắt đầu → hạn
                    .Where(x => !x.IsDeleted && x.CompletedAt == null && x.DueDate != null
                        && (x.DueDate <= today || (x.StartDate != null && x.StartDate <= today))
                        && userIds.Contains(x.OwnerId))
                    .OrderBy(x => x.DueDate).ThenBy(x => x.DueTime == null).ThenBy(x => x.DueTime)
                    .ThenByDescending(x => x.Priority).ThenBy(x => x.SortOrder).ThenBy(x => x.Id)
                    .Select(x => new SummaryTask(x.OwnerId, x.ListId, x.Title, x.Priority, x.DueDate!.Value, x.DueTime))
                    .ToListAsync(ct);

                var finance = financeEnabled
                    ? await _financeSummary.BuildAsync(userIds, today, ct)
                    : new Dictionary<string, string>();
                // Người chỉ có phần Tài chính (không có việc) vẫn nhận tin
                var recipients = tasks.Select(x => x.OwnerId).Concat(finance.Keys).Distinct().ToList();
                if (recipients.Count == 0) return Result.Ok();

                var listIds = tasks.Where(x => x.ListId != null).Select(x => x.ListId!).Distinct().ToList();
                var listNames = await _listRepo.Query(true)
                    .Where(x => listIds.Contains(x.Id) && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

                int sent = 0, failed = 0;
                foreach (var userId in recipients)
                {
                    var message = BuildMessage(today, tasks.Where(x => x.OwnerId == userId).ToList(), listNames);
                    // Phần Tài chính sau phần việc (cách 1 dòng trống), vẫn trong trần độ dài 1 tin
                    if (finance.TryGetValue(userId, out var financeText) && message.Length + 2 + financeText.Length <= MaxMessageLength)
                        message += "\n\n" + financeText;

                    //NOTE: Lỗi gửi cho một chat chỉ ghi log, vẫn gửi tiếp cho người khác.
                    // Không dùng ct của request: cron ngắt kết nối giữa chừng thì vẫn gửi nốt, tránh nửa có nửa không rồi cron gọi lại gửi trùng
                    foreach (var connection in connections.Where(x => x.UserId == userId))
                    {
                        var rs = await _telegramService.SendMessageToChatAsync(connection.ChatId, message, CancellationToken.None);
                        if (rs.IsOk())
                        {
                            sent++;
                        }
                        else
                        {
                            failed++;
                            _logger.Warning("Không gửi được tóm tắt buổi sáng cho user {userId}, chat {chatId}: {message}", userId, connection.ChatId, rs.Message);
                        }
                    }
                }

                _logger.Info("Tóm tắt công việc buổi sáng: gửi {sent}, lỗi {failed}", sent, failed);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi tóm tắt công việc buổi sáng");
                return Result.Exception("Có lỗi xảy ra khi gửi tóm tắt công việc", ex);
            }
        }

        private static string BuildMessage(DateOnly today, List<SummaryTask> tasks, Dictionary<string, string> listNames)
        {
            var overdue = tasks.Where(x => x.DueDate < today).ToList();
            var dueToday = tasks.Where(x => x.DueDate == today).ToList();
            var inProgress = tasks.Where(x => x.DueDate > today).ToList();

            var sb = new StringBuilder();
            //NOTE: Không có việc nào (chỉ có phần Tài chính) thì tiêu đề chung, không ghi "Việc hôm nay"
            sb.Append(tasks.Count > 0 ? "☀️ Việc hôm nay " : "☀️ Buổi sáng ").Append(today.ToString("dd/MM", CultureInfo.InvariantCulture));
            //NOTE: Việc quá hạn kèm ngày hạn để biết đã trễ bao lâu
            AppendGroup(sb, $"⚠️ Quá hạn ({overdue.Count})", overdue, listNames, withDate: true);
            AppendGroup(sb, $"📅 Hôm nay ({dueToday.Count})", dueToday, listNames, withDate: false);
            //NOTE: Việc nhiều ngày đang làm (chưa tới hạn): kèm ngày hạn
            AppendGroup(sb, $"🗓 Đang làm ({inProgress.Count})", inProgress, listNames, withDate: true);
            return sb.ToString();
        }

        private static void AppendGroup(StringBuilder sb, string header, List<SummaryTask> tasks, Dictionary<string, string> listNames, bool withDate)
        {
            if (tasks.Count == 0) return;

            sb.Append("\n\n").Append(header);
            var shown = 0;
            foreach (var task in tasks.Take(MaxLinesPerGroup))
            {
                var line = new StringBuilder();
                line.Append('\n').Append(PriorityIcon(task.Priority)).Append(' ');
                if (withDate) line.Append(task.DueDate.ToString("dd/MM", CultureInfo.InvariantCulture)).Append(' ');
                if (task.DueTime != null) line.Append(task.DueTime.Value.ToString("HH:mm", CultureInfo.InvariantCulture)).Append(' ');
                line.Append(Shorten(task.Title));
                if (task.ListId != null && listNames.TryGetValue(task.ListId, out var listName)) line.Append(" · ").Append(Shorten(listName));

                if (sb.Length + line.Length > MaxMessageLength) break;
                sb.Append(line);
                shown++;
            }

            if (tasks.Count > shown)
                sb.Append("\n… và ").Append(tasks.Count - shown).Append(" việc khác");
        }

        private static string Shorten(string text)
        {
            if (text.Length <= MaxTitleLength) return text;
            var cut = MaxTitleLength - 1;
            //NOTE: Không cắt giữa cặp surrogate (emoji), Telegram từ chối chuỗi UTF-16 lỗi
            if (char.IsHighSurrogate(text[cut - 1])) cut--;
            return string.Concat(text.AsSpan(0, cut), "…");
        }

        private static string PriorityIcon(TodoPriority priority) => priority switch
        {
            TodoPriority.High => "🔴",
            TodoPriority.Medium => "🟠",
            TodoPriority.Low => "🔵",
            _ => "▫️"
        };

        private sealed record SummaryTask(string OwnerId, string? ListId, string Title, TodoPriority Priority, DateOnly DueDate, TimeOnly? DueTime);
    }
}
