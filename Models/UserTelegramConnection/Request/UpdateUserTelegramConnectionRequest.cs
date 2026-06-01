using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateUserTelegramConnectionRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public long ChatId { get; set; }
        [Required]
        public string UserId { get; set; } = default!;
    }
}
