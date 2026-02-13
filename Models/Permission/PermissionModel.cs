namespace ManageLife.Models
{
    public class PermissionModel
    {
        public string Id { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
