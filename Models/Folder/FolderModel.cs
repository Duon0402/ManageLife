namespace ManageLife.Models
{
    public class FolderModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedUser { get; set; } = null!;
        public int PhotoCount { get; set; }
    }
}
