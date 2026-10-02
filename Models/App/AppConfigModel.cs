namespace ManageLife.Models
{
    /// <summary>Cấu hình app mobile đọc lúc khởi động.</summary>
    public class AppConfigModel
    {
        /// <summary>Cờ bật/tắt theo key tính năng của app (vd "todo").</summary>
        public Dictionary<string, bool> Features { get; set; } = new();
    }
}
