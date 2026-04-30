using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Entities
{
    public class UserPermissionEntity
    {
        public string UserId { get; set; } = default!;
        public string PermissionId { get; set; } = default!;
        public UserPermissionStatus Status { get; set; }
    }
}
