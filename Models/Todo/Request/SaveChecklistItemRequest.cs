using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveChecklistItemRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Nội dung không được để trống")]
        [MaxLength(500, ErrorMessage = "Nội dung tối đa 500 ký tự")]
        public string Title { get; set; } = null!;

        public bool IsDone { get; set; }
    }
}
