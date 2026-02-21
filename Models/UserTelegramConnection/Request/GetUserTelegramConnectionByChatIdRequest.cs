using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUserTelegramConnectionByChatIdRequest
    {
        [Required]
        public long ChatId { get; set; } = default!;
    }
}
