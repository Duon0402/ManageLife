using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>
    /// Khoá thiết bị cho đăng nhập bằng vân tay/khuôn mặt: app giữ một bí mật ngẫu nhiên chỉ đọc được sau khi xác thực
    /// sinh trắc, BE chỉ lưu hash. Đăng nhập = gửi deviceId + bí mật → cấp phiên mới.
    /// </summary>
    public class UserDeviceCredentialEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string UserId { get; set; } = default!;

        /// <summary>UUID do app tạo, giữ nguyên theo máy.</summary>
        public string DeviceId { get; set; } = default!;

        /// <summary>Tên máy (header X-Device-Name), vd "Pixel 7 · Android 14".</summary>
        public string? DeviceName { get; set; }

        /// <summary>Hex SHA-256 của bí mật — không lưu bí mật gốc.</summary>
        public string SecretHash { get; set; } = default!;

        /// <summary>Lần cuối đăng nhập bằng khoá này (UTC).</summary>
        public DateTime? LastUsedAt { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
