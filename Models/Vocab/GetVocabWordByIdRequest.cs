using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetVocabWordByIdRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
