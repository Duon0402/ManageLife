using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetVocabWordByIdRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
