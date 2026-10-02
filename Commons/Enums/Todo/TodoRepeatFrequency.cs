namespace ManageLife.Commons
{
    /// <summary>Tần suất lặp của task (dự phòng cho tính năng lặp lại, chưa dùng ở MVP).</summary>
    public enum TodoRepeatFrequency : byte
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Yearly = 3
    }
}
