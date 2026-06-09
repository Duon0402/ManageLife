using ManageLife.Data;
using Microsoft.EntityFrameworkCore;

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

    public class PendingMigrationMiddleware(RequestDelegate next, IConfiguration config)
    {
        private const string MigratePath = "/_migrate";

        public async Task InvokeAsync(HttpContext context, DatabaseState dbState)
        {
            if (!dbState.HasPending)
            {
                await next(context);
                return;
            }

            var path = context.Request.Path.Value ?? "";

            // Handle migration POST — no auth needed, verified by MigrationKey
            if (path.Equals(MigratePath, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method == HttpMethods.Post)
            {
                await HandleMigrateAsync(context, dbState);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";
            var names = string.Join("<br>", dbState.PendingMigrations.Select(System.Net.WebUtility.HtmlEncode));
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
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.75rem}
                        p{color:#555;line-height:1.6;margin-bottom:1rem}
                        code{display:block;font-size:.8rem;color:#888;margin-bottom:1.5rem;word-break:break-all}
                        input[type=password]{width:100%;padding:.6rem .9rem;border:1px solid #ddd;border-radius:8px;font-size:1rem;margin-bottom:.75rem}
                        button{width:100%;padding:.7rem;background:#4b49ac;color:#fff;border:none;border-radius:8px;font-size:1rem;cursor:pointer}
                        button:hover{background:#3d3c8e}
                        .err{color:#d9534f;font-size:.9rem;margin-bottom:.75rem}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <h1>⚙️ Đang cập nhật hệ thống</h1>
                        <p>Cơ sở dữ liệu cần được cập nhật.</p>
                        <code>{{names}}</code>
                        <form method="post" action="{{MigratePath}}">
                            <input type="password" name="key" placeholder="Migration Key" autofocus required>
                            <button type="submit">Chạy Migration</button>
                        </form>
                    </div>
                </body>
                </html>
                """);
        }

        private async Task HandleMigrateAsync(HttpContext context, DatabaseState dbState)
        {
            var form = await context.Request.ReadFormAsync();
            var submittedKey = form["key"].ToString();
            var expectedKey = config["MigrationKey"] ?? "";

            if (string.IsNullOrEmpty(expectedKey) || submittedKey != expectedKey)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(BuildErrorPage("Key không hợp lệ.", dbState));
                return;
            }

            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                dbState.ClearPending();

                context.Response.Redirect("/");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(BuildErrorPage(System.Net.WebUtility.HtmlEncode(ex.Message), dbState));
            }
        }

        private static string BuildErrorPage(string error, DatabaseState dbState)
        {
            var names = string.Join("<br>", dbState.PendingMigrations.Select(System.Net.WebUtility.HtmlEncode));
            return $$"""
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
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.75rem}
                        p{color:#555;line-height:1.6;margin-bottom:1rem}
                        code{display:block;font-size:.8rem;color:#888;margin-bottom:1.5rem;word-break:break-all}
                        input[type=password]{width:100%;padding:.6rem .9rem;border:1px solid #ddd;border-radius:8px;font-size:1rem;margin-bottom:.75rem}
                        button{width:100%;padding:.7rem;background:#4b49ac;color:#fff;border:none;border-radius:8px;font-size:1rem;cursor:pointer}
                        button:hover{background:#3d3c8e}
                        .err{color:#d9534f;font-size:.9rem;margin-bottom:.75rem}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <h1>⚙️ Đang cập nhật hệ thống</h1>
                        <p>Cơ sở dữ liệu cần được cập nhật.</p>
                        <code>{{names}}</code>
                        <p class="err">{{error}}</p>
                        <form method="post" action="{{MigratePath}}">
                            <input type="password" name="key" placeholder="Migration Key" autofocus required>
                            <button type="submit">Chạy Migration</button>
                        </form>
                    </div>
                </body>
                </html>
                """;
        }
    }
}
