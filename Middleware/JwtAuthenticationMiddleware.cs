using ManageLife.Core;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ManageLife.Middleware
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;
            var requireAuthorize = endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null;

            if (allowAnonymous || !requireAuthorize)
            {
                await _next(context);
                return;
            }

            var ct = context.RequestAborted;
            bool stampRejected = false;

            var accessToken = context.Request.Cookies["accessToken"];
            if (accessToken.IsNotEmpty())
            {
                var principal = tokenService.ValidateAccessToken(accessToken);
                if (principal != null)
                {
                    var stampValid = await tokenService.ValidateSecurityStampAsync(principal, ct);
                    if (stampValid)
                    {
                        context.User = principal;
                        await _next(context);
                        return;
                    }
                    // Stamp mismatch: mật khẩu đã đổi, refresh token cũng đã bị revoke
                    stampRejected = true;
                }
            }

            if (!stampRejected)
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (refreshToken.IsNotEmpty())
                {
                    var result = await tokenService.RefreshTokenAsync(refreshToken, ct);
                    if (result.IsOk() && result.Data.AccessToken.IsNotEmpty() && result.Data.RefreshToken.IsNotEmpty())
                    {
                        tokenService.SetTokensCookie(result.Data.AccessToken, result.Data.RefreshToken);
                        var newPrincipal = tokenService.ValidateAccessToken(result.Data.AccessToken);
                        if (newPrincipal != null)
                        {
                            context.User = newPrincipal;
                            await _next(context);
                            return;
                        }
                    }
                }
            }

            tokenService.ClearTokensCookie();

            var isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var returnUrl = context.Request.Path + context.Request.QueryString;

            if (isAjax)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { code = "401", message = "Unauthorized" });
            }
            else
            {
                var loginUrl = $"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                context.Response.Redirect(loginUrl);
            }
        }
    }
}
