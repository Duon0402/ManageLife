using ManageLife.Base;

namespace ManageLife.Entities
{
    public class AlbumFileEntity : EntityBase, ICanCreate
    {
        public string AlbumId { get; set; } = default!;
        public string FileId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
