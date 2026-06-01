using System.ComponentModel.DataAnnotations;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetLanguageByIdRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
