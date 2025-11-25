using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SendMessageRequest
    {
        [Required(ErrorMessage = "Message không được để trống")]
        public string Message { get; set; } = null!;
    }
}
