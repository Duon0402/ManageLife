using ManageLife.Entities;

namespace ManageLife.Models
{
    public class FileModel
    {
        public string Id { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string FileType { get; set; } = null!;

        public long FileSize { get; set; }

        public string Extension { get; set; } = null!;

        // Telegram FileId (NULL khi chưa upload)
        public string? FileId { get; set; }

        // Temp file path (xoá sau khi upload xong)
        public string? TempPath { get; set; }
        public UploadStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedUser { get; set; } = null!;
    }
}
