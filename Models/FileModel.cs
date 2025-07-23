using ManageLife.Base;

namespace ManageLife.Models
{
    public class FileModel
    {
        public string Id { get; set; } = IdHeper.NewId();
        public string FileName { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty; // Unique identifier for the file in storage
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedUser { get; set; } = string.Empty;
    }
}
