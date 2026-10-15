using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IPasswordResetService
    {
        /// <summary>
        /// Gửi mã đặt lại mật khẩu qua Telegram đã liên kết. Luôn trả cùng một câu (không lộ tài khoản có tồn tại hay không).
        /// </summary>
        Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

        /// <summary>Đặt lại mật khẩu bằng mã: thu hồi mọi phiên (mọi máy phải đăng nhập lại).</summary>
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
