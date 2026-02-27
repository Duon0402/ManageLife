namespace ManageLife.Models
{
    public class AddFileToFolderCommand
    {
        public string FolderId { get; set; } = null!;
        public string FileId { get; set; } = null!;
    }
}
