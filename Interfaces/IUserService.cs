using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUserService
    {
        public Task<Result> RegisterAsync(RegisterAccountModel model);

        public Task<Result> LoginAsync(LoginAccountModel model);

        public Task<Result> LogoutAsync(string refreshToken);
    }
}
