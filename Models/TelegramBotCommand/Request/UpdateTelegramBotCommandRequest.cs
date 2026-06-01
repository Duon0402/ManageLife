using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateTelegramBotCommandRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;

        [Required(ErrorMessage = "Command không được để trống")]
        public string Command { get; set; } = default!;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; } = default!;

        public int SortOrder { get; set; }
    }
}
