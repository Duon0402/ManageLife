using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserService
    {
        public Task<Result> RegisterAsync(RegisterAccountRequest model);

        public Task<Result> LoginAsync(LoginAccountRequest model);

        public Task<Result> LogoutAsync(string? refreshToken);

        public Task<Result> ChangePasswordAsync(ChangePasswordRequest request, string? refreshToken);
    }
}
