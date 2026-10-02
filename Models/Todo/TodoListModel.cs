namespace ManageLife.Models
{
    public class TodoListModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }

        /// <summary>Số task chưa xong trong list.</summary>
        public int OpenCount { get; set; }
    }
}
