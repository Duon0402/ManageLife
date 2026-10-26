using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    /// <summary>Cài đặt tóm tắt buổi sáng qua Telegram của chính người dùng.</summary>
    public class UpdateTelegramSummaryRequest : IValidatableRequest
    {
        public bool Enabled { get; set; }

        /// <summary>Giờ gửi theo giờ VN.</summary>
        [Range(0, 23, ErrorMessage = "Giờ gửi không hợp lệ")]
        public int Hour { get; set; }
    }
}
