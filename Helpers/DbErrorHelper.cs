using Microsoft.EntityFrameworkCore;

namespace ManageLife.Helpers
{
    /// <summary>Phân loại lỗi DB dùng chung cho các service.</summary>
    public static class DbErrorHelper
    {
        /// <summary>
        /// Vi phạm unique key của MySQL ("Duplicate entry", SQLSTATE 23000) — so message vì mã số lỗi
        /// có thể khác nhau giữa driver/phiên bản.
        /// Chỉ lỗi này được phép bỏ qua kiểu "người khác đã tạo trước"; mọi DbUpdateException khác
        /// (thiếu bảng do chưa migrate, mất kết nối, deadlock...) đều là lỗi thật phải báo.
        /// </summary>
        public static bool IsUniqueViolation(Exception ex)
        {
            return ex is DbUpdateException dbEx &&
                   dbEx.InnerException?.Message.Contains("Duplicate entry") == true;
        }
    }
}
