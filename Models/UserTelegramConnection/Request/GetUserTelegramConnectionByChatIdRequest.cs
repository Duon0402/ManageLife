using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUserTelegramConnectionByChatIdRequest : IValidatableRequest
    {
        [Required]
        public long ChatId { get; set; } = default!;
    }
}
