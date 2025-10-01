using ManageLife.Base;

namespace ManageLife.Entities
{
    public class FileEntity : EntityBase, ICanCreate
    {
        public string FileName { get; set; } = null!;
        public string FileId { get; set; } = null!; // Unique identifier for the file in storage
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string Extension { get; set; } = null!;
        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
    }
}
