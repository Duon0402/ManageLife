using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateFolderCommand
    {
        [Required(ErrorMessage = "Tên folder không được để trống")]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
