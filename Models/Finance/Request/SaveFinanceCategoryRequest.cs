using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceCategoryRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [EnumDataType(typeof(FinanceCategoryKind), ErrorMessage = "Loại danh mục không hợp lệ")]
        public FinanceCategoryKind Kind { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tên danh mục từ 1 đến 50 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Thiếu icon danh mục")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        public string Icon { get; set; } = null!;

        [Required(ErrorMessage = "Thiếu màu danh mục")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Màu không hợp lệ (dạng #RRGGBB)")]
        public string Color { get; set; } = "#64748B";

        /// <summary>Danh mục cha; GĐ1 phải null.</summary>
        [StringLength(255, ErrorMessage = "Id danh mục cha không hợp lệ")]
        public string? ParentId { get; set; }

        public bool IsArchived { get; set; }
    }
}
