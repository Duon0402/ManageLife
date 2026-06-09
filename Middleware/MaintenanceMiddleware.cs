using ManageLife.Commons;
using ManageLife.Contexts;

namespace ManageLife.Middleware
{
    public class MaintenanceMiddleware(RequestDelegate next)
    {
        private static readonly string[] _allowedPrefixes =
        [
            "/admin",
            "/auth",
            "/api",
        ];

        public async Task InvokeAsync(HttpContext context, ISettingContext settings)
        {
            var path = context.Request.Path.Value ?? "";
            var isAllowed = _allowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                bool maintenanceEnabled;
                try { maintenanceEnabled = await settings.GetBoolAsync(SettingKeys.Maintenance.Enabled); }
                catch { maintenanceEnabled = false; }

                if (maintenanceEnabled)
                {
                    var message = await settings.GetStringAsync(SettingKeys.Maintenance.Message)
                                  ?? "Website đang bảo trì, vui lòng quay lại sau.";

                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync($$"""
                        <!DOCTYPE html>
                        <html lang="vi">
                        <head>
                            <meta charset="utf-8">
                            <meta name="viewport" content="width=device-width,initial-scale=1">
                            <title>Bảo trì hệ thống</title>
                            <style>
                                *{box-sizing:border-box;margin:0;padding:0}
                                body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;min-height:100vh;background:#f0f2f5}
                                .box{background:#fff;border-radius:16px;padding:3rem 2.5rem;text-align:center;max-width:480px;width:90%;box-shadow:0 4px 24px rgba(0,0,0,.08)}
                                .icon{font-size:3.5rem;margin-bottom:1.25rem}
                                h1{color:#1a1a2e;font-size:1.6rem;margin-bottom:.75rem}
                                p{color:#555;line-height:1.6}
                            </style>
                        </head>
                        <body>
                            <div class="box">
                                <div class="icon">🔧</div>
                                <h1>Đang bảo trì</h1>
                                <p>{{System.Net.WebUtility.HtmlEncode(message)}}</p>
                            </div>
                        </body>
                        </html>
                        """);
                    return;
                }
            }

            await next(context);
        }
    }
}
