using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
	public class ImportTranslationExcelRequest
	{
		[Required]
		public IFormFile File { get; set; } = default!;
	}
}
