using ManageLife.Core;

namespace ManageLife.Entities
{
    public class TodoChecklistItemEntity : EntityBase
    {
        public string TaskId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }
    }
}
