using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SendTelegramMessageRequest
    {
        [Required(ErrorMessage = "Message không được để trống")]
        public string Message { get; set; } = null!;
    }
}
