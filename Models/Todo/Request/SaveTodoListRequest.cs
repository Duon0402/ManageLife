using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveTodoListRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng. Bỏ qua khi sửa.</summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Tên danh sách không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên danh sách tối đa 100 ký tự")]
        public string Name { get; set; } = null!;

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Màu không hợp lệ (dạng #RRGGBB)")]
        public string Color { get; set; } = "#4F46E5";

        [MaxLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        public string? Icon { get; set; }
    }
}
