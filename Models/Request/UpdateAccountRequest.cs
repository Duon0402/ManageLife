using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateAccountRequest : IValidatableRequest
    {
        [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [MaxLength(256, ErrorMessage = "Email tối đa 256 ký tự")]
        public string? Email { get; set; }
    }
}
