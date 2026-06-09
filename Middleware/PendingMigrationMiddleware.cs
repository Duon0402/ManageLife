using ManageLife.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

            if (path.Equals(MigratePath, StringComparison.OrdinalIgnoreCase)
                && context.Request.Method == HttpMethods.Post)
            {
                await HandleMigrateAsync(context, dbState);
                return;
            }

            await WritePendingPageAsync(context, dbState, errorMessage: null);
        }

        private async Task HandleMigrateAsync(HttpContext context, DatabaseState dbState)
        {
            var form = await context.Request.ReadFormAsync();
            var submittedKey = form["key"].ToString();
            var storedHash = config["MigrationKeyHash"] ?? "";

            if (string.IsNullOrEmpty(storedHash) || HashKey(submittedKey) != storedHash)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/html; charset=utf-8";
                await WritePendingPageAsync(context, dbState, "Key không hợp lệ.");
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
                await WritePendingPageAsync(context, dbState,
                    System.Net.WebUtility.HtmlEncode(ex.Message));
            }
        }

        private static async Task WritePendingPageAsync(HttpContext context, DatabaseState dbState, string? errorMessage)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";

            var names = string.Join("<br>",
                dbState.PendingMigrations.Select(System.Net.WebUtility.HtmlEncode));

            var errorHtml = errorMessage is null ? "" :
                $"""<p class="err">{errorMessage}</p>""";

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
                        .box{background:#fff;border-radius:16px;padding:2.5rem 2.5rem;text-align:center;max-width:520px;width:90%;box-shadow:0 4px 24px rgba(0,0,0,.08)}
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.6rem}
                        p{color:#555;line-height:1.6;margin-bottom:.75rem}
                        code{display:block;font-size:.78rem;color:#888;margin-bottom:1.25rem;word-break:break-all;text-align:left;background:#f8f8f8;padding:.6rem .8rem;border-radius:6px}
                        a.btn-admin{display:inline-block;padding:.6rem 1.5rem;background:#4b49ac;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;margin-bottom:1.25rem}
                        a.btn-admin:hover{background:#3d3c8e}
                        .divider{display:flex;align-items:center;gap:10px;color:#bbb;font-size:.8rem;margin-bottom:1rem}
                        .divider::before,.divider::after{content:'';flex:1;border-top:1px solid #eee}
                        details summary{cursor:pointer;color:#999;font-size:.82rem;user-select:none;list-style:none;padding:.4rem 0}
                        details summary:hover{color:#666}
                        details[open] summary{color:#555}
                        .key-form{margin-top:.75rem}
                        .key-wrap{position:relative;margin-bottom:.75rem}
                        .key-wrap input{width:100%;padding:.6rem .9rem;padding-right:2.5rem;border:1px solid #ddd;border-radius:8px;font-size:.95rem}
                        .key-wrap .eye{position:absolute;right:.75rem;top:50%;transform:translateY(-50%);background:none;border:none;cursor:pointer;color:#aaa;padding:0;line-height:1}
                        .key-wrap .eye:hover{color:#555}
                        button[type=submit]{width:100%;padding:.65rem;background:#dc3545;color:#fff;border:none;border-radius:8px;font-size:.95rem;cursor:pointer;font-weight:600}
                        button[type=submit]:hover{background:#b02a37}
                        .err{color:#d9534f;font-size:.88rem;margin-bottom:.75rem;text-align:left;background:#fff5f5;padding:.5rem .75rem;border-radius:6px}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <h1>⚙️ Đang cập nhật hệ thống</h1>
                        <p>Cơ sở dữ liệu cần được cập nhật trước khi tiếp tục.</p>
                        <code>{{names}}</code>

                        <a href="/auth/login" class="btn-admin">🔐 Đăng nhập Admin</a>

                        <div class="divider">hoặc dùng Migration Key</div>

                        <details>
                            <summary>▶ Migrate khẩn cấp (khi không đăng nhập được)</summary>
                            <div class="key-form">
                                {{errorHtml}}
                                <form method="post" action="{{MigratePath}}">
                                    <div class="key-wrap">
                                        <input type="password" id="mkey" name="key" placeholder="Migration Key" autocomplete="off" required>
                                        <button type="button" class="eye" onclick="toggleKey()">👁</button>
                                    </div>
                                    <button type="submit">Chạy Migration</button>
                                </form>
                            </div>
                        </details>
                    </div>
                    <script>
                        function toggleKey() {
                            var i = document.getElementById('mkey');
                            i.type = i.type === 'password' ? 'text' : 'password';
                        }
                    </script>
                </body>
                </html>
                """);
        }

        internal static string HashKey(string key) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
    }
}
