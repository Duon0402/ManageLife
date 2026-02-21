using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateUserTelegramConnectionRequest
    {
        [Required]
        public long ChatId { get; set; } = default!;
        [Required]
        public string UserId { get; set; } = default!;
    }
}
