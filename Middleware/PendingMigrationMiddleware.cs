namespace ManageLife.Middleware
{
    public class DatabaseState
    {
        public IReadOnlyList<string> PendingMigrations { get; private set; } = [];
        public bool HasPending => PendingMigrations.Count > 0;

        public void SetPending(IEnumerable<string> migrations) =>
            PendingMigrations = migrations.ToList().AsReadOnly();

        public void ClearPending() =>
            PendingMigrations = [];
    }

    public class PendingMigrationMiddleware(RequestDelegate next)
    {
        private static readonly string[] _adminPrefixes = ["/admin", "/api/admin"];

        public async Task InvokeAsync(HttpContext context, DatabaseState dbState)
        {
            if (!dbState.HasPending)
            {
                await next(context);
                return;
            }

            var path = context.Request.Path.Value ?? "";
            if (_adminPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await next(context);
                return;
            }

            var names = string.Join(", ", dbState.PendingMigrations);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync($$"""
                <!DOCTYPE html>
                <html lang="vi">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <title>Đang cập nhật hệ thống</title>
                    <style>
                        *{box-sizing:border-box;margin:0;padding:0}
                        body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;min-height:100vh;background:#f0f2f5}
                        .box{background:#fff;border-radius:16px;padding:3rem 2.5rem;text-align:center;max-width:520px;width:90%;box-shadow:0 4px 24px rgba(0,0,0,.08)}
                        .icon{font-size:3rem;margin-bottom:1.25rem}
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.75rem}
                        p{color:#555;line-height:1.6;margin-bottom:.5rem}
                        code{font-size:.8rem;color:#888;word-break:break-all}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <div class="icon">⚙️</div>
                        <h1>Đang cập nhật hệ thống</h1>
                        <p>Cơ sở dữ liệu cần được cập nhật trước khi tiếp tục sử dụng.</p>
                        <p>Vui lòng liên hệ quản trị viên để hoàn tất quá trình.</p>
                        <br>
                        <code>{{System.Net.WebUtility.HtmlEncode(names)}}</code>
                    </div>
                </body>
                </html>
                """);
        }
    }
}
