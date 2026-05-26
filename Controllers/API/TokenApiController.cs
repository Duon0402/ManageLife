using ManageLife.Core;
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
        public async Task<IActionResult> CleanupRefreshTokens(CancellationToken ct)
        {
            var rs = await _tokenService.CleanupRefreshTokensAsync(ct: ct);

            if (rs.IsOk())
                return Ok();

            return BadRequest(rs.Message);
        }
    }
}
