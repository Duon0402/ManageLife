using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ManageLife.Controllers.API
{
    [Route("api/auth")]
    public class AuthApiController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthApiController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<Result<AuthTokenModel>> Login([FromBody] LoginAccountRequest model, CancellationToken ct)
        {
            return await _userService.LoginAsync(model, ct);
        }

        [HttpPost("refresh")]
        public async Task<Result<AuthTokenModel>> Refresh([FromBody] RefreshTokenRequest model, CancellationToken ct)
        {
            return await _tokenService.RefreshTokenAsync(model.RefreshToken, ct);
        }
    }
}
