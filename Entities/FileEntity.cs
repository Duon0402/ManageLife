using ManageLife.Base;

namespace ManageLife.Entities
{
    public class FileEntity : EntityBase, ICanCreate
    {
        public string FileName { get; set; } = null!;

        public string FileType { get; set; } = null!;

        public long FileSize { get; set; }

        public string Extension { get; set; } = null!;

        // Telegram FileId (NULL khi chưa upload)
        public string? FileId { get; set; }

        // Temp file path (xoá sau khi upload xong)
        public string? TempPath { get; set; }

        public UploadStatus Status { get; set; }

        public string CreatedUser { get; set; } = null!;

        public DateTime CreatedTime { get; set; }
    }

    public enum UploadStatus
    {
        Pending = 0,
        Uploading = 1,
        Completed = 2,
        Failed = 3
    }
}
