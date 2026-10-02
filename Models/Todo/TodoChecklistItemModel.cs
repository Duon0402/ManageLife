namespace ManageLife.Models
{
    public class TodoChecklistItemModel
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }
    }
}
