namespace ManageLife.Models
{
    public class FileModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileId { get; set; } // Unique identifier for the file in storage
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedUser { get; set; }
    }
}
