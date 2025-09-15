namespace ManageLife.Base
{
    public class FileUploaderViewOptions
    {
        public string? Id { get; set; }
        public string Title { get; set; } = "File Uploader";
        public string Width { get; set; } = "100%";

        public string? CssClass { get; set; }

        public int? MaxFileCount { get; set; }
        public long? MaxFileSize { get; set; }

        public string Accept { get; set; } = "*/*";
        public string UploadUrl { get; set; } = "/filestorage/upload";

        public UploadMode UploadMode { get; set; } = UploadMode.Instant;

        public bool AllowRemove { get; set; } = true;
        public bool AllowMultiFileUpload { get; set; } = true;

        public Dictionary<string, string>? AdditionalData { get; set; }
    }

    public enum UploadMode
    {
        Instant,
        OnButtonClick,
    }
}
