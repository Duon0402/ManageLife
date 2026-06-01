using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUserTelegramConnectionByUserIdRequest : IValidatableRequest
    {
        [Required]
        public string UserId { get; set; } = default!;
    }
}
