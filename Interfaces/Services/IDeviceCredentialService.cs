using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IDeviceCredentialService
    {
        /// <summary>Bật đăng nhập bằng vân tay trên máy này: lưu hash bí mật kèm mã thiết bị.</summary>
        Task<Result> SaveAsync(DeviceCredentialRequest request, CancellationToken ct = default);

        /// <summary>Tắt đăng nhập bằng vân tay trên máy này (xoá khoá trên máy + trên BE).</summary>
        Task<Result> DeleteAsync(string deviceId, CancellationToken ct = default);

        /// <summary>Đăng nhập bằng khoá thiết bị (dùng được cả khi đã đăng xuất/hết phiên).</summary>
        Task<Result<AuthTokenModel>> LoginAsync(DeviceLoginRequest request, CancellationToken ct = default);
    }
}
