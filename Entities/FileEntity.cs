using ManageLife.Base;

namespace ManageLife.Entities
{
    public class FileEntity : EntityBase
    {
        public string FileName { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty; // Unique identifier for the file in storage
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
