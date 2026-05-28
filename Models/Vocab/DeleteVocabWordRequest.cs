using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteVocabWordRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
