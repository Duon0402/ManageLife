using ManageLife.Base;

namespace ManageLife.Entities
{
	public class FileEntity : EntityBase
	{
		public string FileName { get; set; } = null!;
		public string FileId { get; set; } = null!; // Unique identifier for the file in storage
		public string FileType { get; set; } = null!;
		public long FileSize { get; set; }
	}
}
