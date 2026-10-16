namespace ManageLife.Models
{
    /// <summary>Cấu hình app mobile đọc lúc khởi động.</summary>
    public class AppConfigModel
    {
        /// <summary>Cờ bật/tắt theo key tính năng của app (vd "todo").</summary>
        public Dictionary<string, bool> Features { get; set; } = new();

        /// <summary>Từ khoá ô thêm nhanh việc theo nhóm (today, tomorrow, dayAfter, nextWeek, priorityHigh/Medium/Low). Nhóm trống = app dùng mặc định.</summary>
        public Dictionary<string, List<string>> QuickAdd { get; set; } = new();

        /// <summary>Từ khoá ô ghi nhanh thu chi theo DefaultKey của danh mục (food, salary…). Nhóm trống = app chỉ khớp theo tên.</summary>
        public Dictionary<string, List<string>> FinanceKeywords { get; set; } = new();
    }
}
