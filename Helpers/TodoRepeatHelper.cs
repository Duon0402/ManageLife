using ManageLife.Commons;

namespace ManageLife.Helpers
{
    /// <summary>Tính ngày hạn lần kế tiếp của việc lặp lại.</summary>
    public static class TodoRepeatHelper
    {
        /// <summary>Bit của một ngày trong RepeatWeekdays: T2=1, T3=2, ... CN=64.</summary>
        public static byte WeekdayBit(DayOfWeek day) => (byte)(1 << (((int)day + 6) % 7));

        /// <summary>
        /// Ngày hạn lần kế tiếp: sau <paramref name="due"/> và không sớm hơn <paramref name="today"/>
        /// (hoàn thành trễ thì lần sau không rơi vào quá khứ). null = hết chuỗi (vượt RepeatUntil).
        /// </summary>
        /// <param name="monthDay">Lặp hằng tháng/năm: ngày gốc (1–31); null = ngày của <paramref name="due"/>.</param>
        public static DateOnly? NextDueDate(
            DateOnly due, TodoRepeatFrequency frequency, int interval, byte? weekdays, DateOnly? until, DateOnly today,
            byte? monthDay = null)
        {
            try
            {
                return Compute(due, frequency, interval, weekdays, until, today, monthDay ?? (byte)due.Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                //NOTE: Vượt khỏi khoảng DateOnly → coi như hết chuỗi
                return null;
            }
        }

        private static DateOnly? Compute(
            DateOnly due, TodoRepeatFrequency frequency, int interval, byte? weekdays, DateOnly? until, DateOnly today,
            byte monthDay)
        {
            interval = Math.Max(1, interval);
            var earliest = today > due ? today : due.AddDays(1);

            DateOnly next;
            switch (frequency)
            {
                case TodoRepeatFrequency.Daily:
                {
                    // Bước nhảy nhỏ nhất (≥ 1) để tới được earliest
                    var steps = Math.Max(1, (int)Math.Ceiling((earliest.DayNumber - due.DayNumber) / (double)interval));
                    next = due.AddDays(steps * interval);
                    break;
                }
                case TodoRepeatFrequency.Weekly:
                {
                    var mask = weekdays is > 0 ? weekdays.Value : WeekdayBit(due.DayOfWeek);
                    var anchorWeek = WeekStart(due);
                    next = earliest;
                    // Trong 7 * interval ngày liên tiếp luôn có ít nhất 1 ngày hợp lệ (mask khác 0)
                    for (var i = 0; i < 7 * interval; i++, next = next.AddDays(1))
                    {
                        var weeks = (WeekStart(next).DayNumber - anchorWeek.DayNumber) / 7;
                        if (weeks % interval == 0 && (mask & WeekdayBit(next.DayOfWeek)) != 0) break;
                    }
                    break;
                }
                case TodoRepeatFrequency.Monthly:
                {
                    // Theo ngày gốc: tháng ngắn lấy ngày cuối tháng nhưng tháng sau trở lại ngày gốc (31/1 → 28/2 → 31/3)
                    var n = 1;
                    do next = MonthAhead(due, n++ * interval, monthDay); while (next < earliest);
                    break;
                }
                default:
                {
                    var n = 1;
                    do next = MonthAhead(due, n++ * interval * 12, monthDay); while (next < earliest);
                    break;
                }
            }

            return until.HasValue && next > until.Value ? null : next;
        }

        /// <summary>Cùng ngày gốc của tháng cách <paramref name="due"/> <paramref name="months"/> tháng (kẹp về cuối tháng).</summary>
        private static DateOnly MonthAhead(DateOnly due, int months, byte monthDay)
        {
            var first = new DateOnly(due.Year, due.Month, 1).AddMonths(months);
            return new DateOnly(first.Year, first.Month, Math.Min(monthDay, DateTime.DaysInMonth(first.Year, first.Month)));
        }

        private static DateOnly WeekStart(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }
}
