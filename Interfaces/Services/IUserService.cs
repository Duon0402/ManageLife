using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserService
    {
        Task<Result<AuthTokenModel>> RegisterAsync(RegisterAccountRequest model, CancellationToken ct = default);

        Task<Result<AuthTokenModel>> LoginAsync(LoginAccountRequest model, CancellationToken ct = default);

        Task<Result> LogoutAsync(string? refreshToken, CancellationToken ct = default);

        Task<Result<AuthTokenModel>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);

        Task<Result<AccountModel>> GetMyAccountAsync(CancellationToken ct = default);

        Task<Result<AccountModel>> UpdateMyAccountAsync(UpdateAccountRequest request, CancellationToken ct = default);

        #region Admin
        Task<Result<List<UserModel>>> GetListUsersAsync(CancellationToken ct = default);
        Task<Result<UserModel>> GetUserByIdAsync(GetUserByIdRequest request, CancellationToken ct = default);
        #endregion
    }
}
