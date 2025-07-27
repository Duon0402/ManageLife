using ManageLife.Base;

namespace ManageLife.Models
{
	public class FileModel
	{
		public string Id { get; set; } = null!;
		public string FileName { get; set; } = null!;
		public string FileId { get; set; } = null!; // Unique identifier for the file in storage
		public string FileType { get; set; } = null!;
		public long FileSize { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedUser { get; set; } = null!;
	}
}
