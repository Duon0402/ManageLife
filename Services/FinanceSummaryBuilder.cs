using ManageLife.Commons;
using ManageLife.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace ManageLife.Services
{
    /// <summary>
    /// Dựng đoạn Tài chính cho tin Telegram buổi sáng (cron ẩn danh: không dùng user context, truyền userIds). Mỗi bảng chỉ
    /// truy vấn 1 lần cho mọi người dùng. Ngày theo giờ VN; giao dịch lưu UTC nên mốc ngày VN đổi sang UTC (VN = UTC+7).
    /// </summary>
    public class FinanceSummaryBuilder : IFinanceSummaryBuilder
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private const int ActiveDays = 31;
        private const int MaxNames = 5;
        private const int MaxNameLength = 40;

        private readonly IFinanceTransactionRepository _transactionRepo;
        private readonly IFinanceBudgetRepository _budgetRepo;
        private readonly IFinanceRecurringRepository _recurringRepo;
        private readonly IFinanceDebtRepository _debtRepo;

        public FinanceSummaryBuilder(
            IFinanceTransactionRepository transactionRepo,
            IFinanceBudgetRepository budgetRepo,
            IFinanceRecurringRepository recurringRepo,
            IFinanceDebtRepository debtRepo)
        {
            _transactionRepo = transactionRepo;
            _budgetRepo = budgetRepo;
            _recurringRepo = recurringRepo;
            _debtRepo = debtRepo;
        }

        public async Task<Dictionary<string, string>> BuildAsync(IReadOnlyCollection<string> userIds, DateOnly today, CancellationToken ct = default)
        {
            var ids = userIds.Distinct().ToList();
            var result = new Dictionary<string, string>();
            if (ids.Count == 0) return result;

            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var activeFrom = Utc(today.AddDays(-ActiveDays));
            var yesterdayFrom = Utc(today.AddDays(-1));
            var todayFrom = Utc(today);
            var monthFrom = Utc(monthStart);

            // Người có giao dịch trong 31 ngày qua (đang dùng Tài chính)
            var active = (await _transactionRepo.Query(true)
                    .Where(x => ids.Contains(x.OwnerId) && !x.IsDeleted && x.OccurredAt >= activeFrom)
                    .Select(x => x.OwnerId).Distinct().ToListAsync(ct))
                .ToHashSet();

            // Chi hôm qua / tháng này (tới đầu hôm nay)
            var spending = await _transactionRepo.Query(true)
                .Where(x => ids.Contains(x.OwnerId) && !x.IsDeleted && x.Kind == FinanceTransactionKind.Expense
                    && x.OccurredAt >= monthFrom.AddDays(-1) && x.OccurredAt < todayFrom)
                .Select(x => new { x.OwnerId, x.Amount, x.OccurredAt })
                .ToListAsync(ct);

            var budgets = await _budgetRepo.Query(true)
                .Where(x => ids.Contains(x.OwnerId) && !x.IsDeleted && x.CategoryId == "" && x.PeriodKind == 1
                    && x.PeriodStart == monthStart)
                .Select(x => new { x.OwnerId, x.Amount })
                .ToListAsync(ct);

            var recurrings = await _recurringRepo.Query(true)
                .Where(x => ids.Contains(x.OwnerId) && !x.IsDeleted && !x.IsPaused && x.NextDate != null && x.NextDate <= today)
                .OrderBy(x => x.NextDate).ThenBy(x => x.Name)
                .Select(x => new { x.OwnerId, x.Name })
                .ToListAsync(ct);

            var debts = await _debtRepo.Query(true)
                .Where(x => ids.Contains(x.OwnerId) && !x.IsDeleted && !x.IsClosed && x.DueDate != null && x.DueDate <= today)
                .OrderBy(x => x.DueDate)
                .Select(x => new { x.Id, x.OwnerId, x.Direction, x.PersonName, x.Amount })
                .ToListAsync(ct);
            var debtIds = debts.Select(x => x.Id).ToList();
            var paid = debtIds.Count == 0
                ? new Dictionary<string, long>()
                : await _transactionRepo.Query(true)
                    .Where(x => !x.IsDeleted && x.Kind == FinanceTransactionKind.DebtPayment && x.DebtId != null && debtIds.Contains(x.DebtId))
                    .GroupBy(x => x.DebtId!)
                    .Select(g => new { DebtId = g.Key, Paid = g.Sum(x => Math.Abs(x.Amount)) })
                    .ToDictionaryAsync(x => x.DebtId, x => x.Paid, ct);

            foreach (var userId in ids)
            {
                var userRecurrings = recurrings.Where(x => x.OwnerId == userId).Select(x => Shorten(x.Name)).ToList();
                var userDebts = debts.Where(x => x.OwnerId == userId).ToList();
                if (!active.Contains(userId) && userRecurrings.Count == 0 && userDebts.Count == 0) continue;

                var sb = new StringBuilder("💰 Tài chính");
                if (active.Contains(userId))
                {
                    var userSpending = spending.Where(x => x.OwnerId == userId).ToList();
                    var yesterday = userSpending.Where(x => x.OccurredAt >= yesterdayFrom).Sum(x => x.Amount);
                    var month = userSpending.Where(x => x.OccurredAt >= monthFrom).Sum(x => x.Amount);
                    sb.Append("\nHôm qua chi: ").Append(Money(yesterday));
                    var budget = budgets.FirstOrDefault(x => x.OwnerId == userId);
                    sb.Append("\nTháng này: ").Append(Money(month));
                    if (budget != null && budget.Amount > 0)
                    {
                        var percent = (int)Math.Round(month * 100.0 / budget.Amount);
                        sb.Append(" / ngân sách ").Append(Money(budget.Amount)).Append($" ({percent}%)");
                    }
                }

                if (userRecurrings.Count > 0)
                {
                    sb.Append("\n🔁 Cần ghi: ").Append(string.Join(", ", userRecurrings.Take(MaxNames)));
                    if (userRecurrings.Count > MaxNames) sb.Append($" và {userRecurrings.Count - MaxNames} khoản khác");
                }

                foreach (var debt in userDebts.Take(MaxNames))
                {
                    var remaining = Math.Max(0, debt.Amount - paid.GetValueOrDefault(debt.Id));
                    var name = Shorten(debt.PersonName);
                    sb.Append("\n🤝 Tới hạn: ")
                        .Append(debt.Direction == 0 ? $"{name} trả {Money(remaining)}" : $"trả {name} {Money(remaining)}");
                }

                result[userId] = sb.ToString();
            }

            return result;
        }

        /// <summary>00:00 ngày VN → UTC.</summary>
        private static DateTime Utc(DateOnly vnDate) =>
            DateTime.SpecifyKind(vnDate.ToDateTime(TimeOnly.MinValue) - VnOffset, DateTimeKind.Utc);

        private static string Money(long amount) => amount.ToString("N0", Vi) + " đ";

        private static string Shorten(string text) =>
            text.Length <= MaxNameLength ? text : string.Concat(text.AsSpan(0, MaxNameLength - 1), "…");
    }
}
