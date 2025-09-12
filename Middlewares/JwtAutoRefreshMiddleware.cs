using ManageLife.Interfaces;
using System.Security.Claims;

public class JwtAutoRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAutoRefreshMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        var accessToken = context.Request.Cookies["accessToken"];
        var refreshToken = context.Request.Cookies["refreshToken"];

        ClaimsPrincipal? principal = null;

        if (!string.IsNullOrEmpty(accessToken))
        {
            principal = tokenService.ValidateAccessToken(accessToken);
        }

        if (principal == null && !string.IsNullOrEmpty(refreshToken))
        {
            var result = await tokenService.RefreshTokenAsync(refreshToken);
            if (result.IsOk())
            {
                var newAccessToken = result.Data.AccessToken;
                principal = tokenService.ValidateAccessToken(newAccessToken);
            }
            else
            {
                tokenService.ClearTokensCookie();
            }
        }

        if (principal != null)
        {
            context.User = principal;
        }

        await _next(context);
    }
}
