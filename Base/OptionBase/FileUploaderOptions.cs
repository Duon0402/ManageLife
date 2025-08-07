namespace ManageLife.Base
{
    public class FileUploaderOptions
    {
        public string? Id { get; set; }
        public string Title { get; set; } = "File Uploader";
        public string? Width { get; set; }
        public int MaxFileCount { get; set; } = 4;
        public string Accept { get; set; } = "*/*";
        public string UploadUrl { get; set; } = "/filestorage/upload";
    }
}
