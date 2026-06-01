using ManageLife.Commons;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class ChangeTaskStatusRequest
    {
        [Required(ErrorMessage = "Id không được để trống")]
        public string Id { get; set; } = null!;

        public TodoStatus Status { get; set; }
    }
}
