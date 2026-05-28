using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class LookupWordRequest
    {
        [Required]
        public string Word { get; set; } = default!;
    }
}
