namespace ManageLife.Models
{
    public class FolderFileItemModel
    {
        public string FileId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        /// <summary>URL để stream file qua FileStorageController.GetFile</summary>
        public string FileUrl { get; set; } = null!;
    }
}
