using ManageLife.Base;

namespace ManageLife.Entities
{
    public class FileEntity : EntityBase, ICanCreate
    {
        public string FileName { get; set; } = default!;

        public string FileType { get; set; } = default!;

        public long FileSize { get; set; }

        public string Extension { get; set; } = default!;

        // Telegram FileId (default khi chưa upload)
        public string? FileId { get; set; }

        // Temp file path (xoá sau khi upload xong)
        public string? TempPath { get; set; }

        public UploadStatus Status { get; set; }

        public string CreatedUser { get; set; } = default!;

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
