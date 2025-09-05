using ManageLife.Entities;

namespace ManageLife.Models
{
    public class BulkInsertPermissionsRequest
    {
        public List<PermissionEntity> Permissions { get; set; } = new List<PermissionEntity>();
    }
}
