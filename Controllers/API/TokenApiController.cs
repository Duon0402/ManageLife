using ManageLife.Base;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.API
{
    [Route("api/token")]
    public class TokenApiController : ApiControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenApiController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("cleanup-refresh-tokens")]
        public async Task<IActionResult> CleanupRefreshTokens()
        {
            var rs = await _tokenService.CleanupRefreshTokensAsync();

            if (rs.IsOk())
            {
                return Ok();
            }

            return BadRequest(rs.Message);
        }
    }
}
