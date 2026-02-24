using ManageLife.Base;

namespace ManageLife.Entities
{
    public class AlbumEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? CoverPhotoId { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
