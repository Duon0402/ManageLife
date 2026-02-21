using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteUserTelegramConnectionRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
