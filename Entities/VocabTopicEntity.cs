using ManageLife.Core;

namespace ManageLife.Entities
{
    public class VocabTopicEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public string OwnerId { get; set; } = default!;
        public bool IsPublic { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
