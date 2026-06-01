using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
	public class ImportTranslationExcelRequest : IValidatableRequest
	{
		[Required]
		public IFormFile File { get; set; } = default!;
	}
}
