using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateUserTelegramConnectionRequest : IValidatableRequest
    {
        [Required]
        public long ChatId { get; set; } = default!;
        [Required]
        public string UserId { get; set; } = default!;
    }
}
