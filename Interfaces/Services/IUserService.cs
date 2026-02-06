using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserService
    {
        Task<Result> RegisterAsync(RegisterAccountRequest model);

        Task<Result> LoginAsync(LoginAccountRequest model);

        Task<Result> LogoutAsync(string? refreshToken);

        Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken);

        #region Admin
        Task<Result<List<UserModel>>> GetListUsersAsync();
        Task<Result<UserModel>> GetUserIdAsync(GetUserIdRequest request);
        #endregion
    }
}
