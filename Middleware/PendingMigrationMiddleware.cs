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
        private static readonly string[] _allowedPrefixes =
            ["/admin", "/auth", "/api"];

        public async Task InvokeAsync(HttpContext context, DatabaseState dbState)
        {
            if (!dbState.HasPending)
            {
                await next(context);
                return;
            }

            var path = context.Request.Path.Value ?? "";

            if (_allowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await next(context);
                return;
            }

            await WritePendingPageAsync(context);
        }

        private static async Task WritePendingPageAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync("""
                <!DOCTYPE html>
                <html lang="vi">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <title>Website đang bảo trì</title>
                    <style>
                        *{box-sizing:border-box;margin:0;padding:0}
                        body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;min-height:100vh;background:#f0f2f5}
                        .box{background:#fff;border-radius:16px;padding:2.5rem;text-align:center;max-width:480px;width:90%;box-shadow:0 4px 24px rgba(0,0,0,.08)}
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.6rem}
                        p{color:#555;line-height:1.6;margin-bottom:.75rem}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <h1>⚙️ Website đang bảo trì</h1>
                        <p>Hệ thống đang được cập nhật, vui lòng quay lại sau ít phút.</p>
                    </div>
                </body>
                </html>
                """);
        }
    }
}
