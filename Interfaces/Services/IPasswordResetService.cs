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

        /// <summary>Kênh đặt lại mật khẩu của tên đăng nhập: telegram hoặc none (chưa liên kết → liên hệ admin).</summary>
        Task<Result<PasswordResetOptionModel>> GetResetOptionAsync(PasswordResetOptionRequest request, CancellationToken ct = default);

        /// <summary>Kiểm tra mã đặt lại mật khẩu (nhập đúng mã mới cho nhập mật khẩu mới).</summary>
        Task<Result> VerifyCodeAsync(VerifyResetCodeRequest request, CancellationToken ct = default);

        /// <summary>Đặt lại mật khẩu bằng mã: thu hồi mọi phiên (mọi máy phải đăng nhập lại).</summary>
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
