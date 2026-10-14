namespace ManageLife.Models
{
    /// <summary>
    /// Cấu hình công khai cho màn đăng nhập (chưa đăng nhập nên không gọi được /api/app/config).
    /// </summary>
    public class AppPublicConfigModel
    {
        /// <summary>Đăng ký tài khoản đang mở (admin tắt bằng setting thì ẩn link "Tạo tài khoản").</summary>
        public bool RegistrationEnabled { get; set; }
    }
}
