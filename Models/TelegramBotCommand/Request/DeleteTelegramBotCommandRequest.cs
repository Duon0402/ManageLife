using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteTelegramBotCommandRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
