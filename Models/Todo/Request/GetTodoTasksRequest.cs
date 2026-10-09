using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetTodoTasksRequest : IValidatableRequest
    {
        /// <summary>today | upcoming | inbox | list | completed | reminders (việc chưa xong có nhắc trong 30 ngày tới, để app đặt thông báo) | search (theo Query).</summary>
        [Required(ErrorMessage = "Chế độ xem không được để trống")]
        [RegularExpression("^(today|upcoming|inbox|list|completed|reminders|search)$", ErrorMessage = "Chế độ xem không hợp lệ")]
        public string View { get; set; } = null!;

        /// <summary>Từ khoá khi View = search: tìm trong tiêu đề và ghi chú (gồm cả việc đã xong).</summary>
        [MaxLength(100, ErrorMessage = "Từ khoá tối đa 100 ký tự")]
        public string? Query { get; set; }

        /// <summary>Bắt buộc khi View = list; tuỳ chọn khi View = completed.</summary>
        public string? ListId { get; set; }

        /// <summary>Bắt đầu từ 0.</summary>
        [Range(0, 10000, ErrorMessage = "Trang không hợp lệ")]
        public int PageIndex { get; set; }

        [Range(1, 100, ErrorMessage = "Số bản ghi mỗi trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = PaginationConst.DefaultPageSize;
    }
}
