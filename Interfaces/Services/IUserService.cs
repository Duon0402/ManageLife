using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserService
    {
        Task<Result> RegisterAsync(RegisterAccountRequest model, CancellationToken ct = default);

        Task<Result> LoginAsync(LoginAccountRequest model, CancellationToken ct = default);

        Task<Result> LogoutAsync(string? refreshToken, CancellationToken ct = default);

        Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken, CancellationToken ct = default);

        #region Admin
        Task<Result<List<UserModel>>> GetListUsersAsync(CancellationToken ct = default);
        Task<Result<UserModel>> GetUserByIdAsync(GetUserByIdRequest request, CancellationToken ct = default);
        #endregion
    }
}
