using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceBudgetRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        /// <summary>Danh mục chi; null = hạn mức tổng cả kỳ.</summary>
        [StringLength(255, ErrorMessage = "Id danh mục không hợp lệ")]
        public string? CategoryId { get; set; }

        /// <summary>Loại kỳ: 0 tuần, 1 tháng, 3 năm (2 quý và 4 khoảng tuỳ chọn đã bỏ).</summary>
        [Range(0, 3, ErrorMessage = "Loại kỳ không hợp lệ")]
        public byte PeriodKind { get; set; }

        /// <summary>Ngày đầu kỳ ('YYYY-MM-DD').</summary>
        public DateOnly PeriodStart { get; set; }

        /// <summary>Ngày cuối kỳ ('YYYY-MM-DD').</summary>
        public DateOnly PeriodEnd { get; set; }

        [Range(1, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long Amount { get; set; }
    }
}
