using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUserTelegramConnectionByUserIdRequest
    {
        [Required]
        public string UserId { get; set; } = default!;
    }
}
