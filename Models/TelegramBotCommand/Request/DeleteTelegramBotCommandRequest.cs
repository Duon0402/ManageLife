using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteTelegramBotCommandRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
