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
        public static DateOnly? NextDueDate(
            DateOnly due, TodoRepeatFrequency frequency, int interval, byte? weekdays, DateOnly? until, DateOnly today)
        {
            try
            {
                return Compute(due, frequency, interval, weekdays, until, today);
            }
            catch (ArgumentOutOfRangeException)
            {
                //NOTE: Vượt khỏi khoảng DateOnly → coi như hết chuỗi
                return null;
            }
        }

        private static DateOnly? Compute(
            DateOnly due, TodoRepeatFrequency frequency, int interval, byte? weekdays, DateOnly? until, DateOnly today)
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
                    //NOTE: Tính từ chính ngày hạn hiện tại; ngày 31 sang tháng ngắn bị kẹp về cuối tháng (31/1 → 28/2 → 28/3)
                    var n = 1;
                    do next = due.AddMonths(n++ * interval); while (next < earliest);
                    break;
                }
                default:
                {
                    var n = 1;
                    do next = due.AddYears(n++ * interval); while (next < earliest);
                    break;
                }
            }

            return until.HasValue && next > until.Value ? null : next;
        }

        private static DateOnly WeekStart(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }
}
