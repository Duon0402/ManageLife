using System.ComponentModel.DataAnnotations;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class DeleteLanguageRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
