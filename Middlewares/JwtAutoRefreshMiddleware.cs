using ManageLife.Interfaces;

public class JwtAutoRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAutoRefreshMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        var accessToken = context.Request.Cookies["accessToken"];
        var refreshToken = context.Request.Cookies["refreshToken"];

        if ((context.User?.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(accessToken))
            && !string.IsNullOrEmpty(refreshToken))
        {
            var result = await tokenService.RefreshTokenAsync(refreshToken);
            if (result.IsOk())
            {
                var newAccessToken = context.Request.Cookies["accessToken"];
                var principal = tokenService.ValidateAccessToken(newAccessToken);
                if (principal != null)
                    context.User = principal;
            }
        }

        await _next(context);
    }
}
