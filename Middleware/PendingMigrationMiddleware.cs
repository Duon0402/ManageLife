using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Helpers;
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

    public class PendingMigrationMiddleware(RequestDelegate next)
    {
        private const string MigratePath = "/_migrate";
        private const string MigratePermission = "Admin.Database.Update";

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

            await WritePendingPageAsync(context, dbState, null);
        }

        private async Task HandleMigrateAsync(HttpContext context, DatabaseState dbState)
        {
            var form = await context.Request.ReadFormAsync();
            var username = form["username"].ToString().Trim();
            var password = form["password"].ToString();

            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var user = await db.Users
                    .Where(u => u.UserName == username && u.IsActive && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (user is null)
                {
                    await WritePendingPageAsync(context, dbState, "Tài khoản hoặc mật khẩu không đúng.");
                    return;
                }

                var passwordValid = PasswordHelper.IsLegacyHash(user.HashPassword)
                    ? PasswordHelper.VerifyLegacy(password, user.HashPassword)
                    : PasswordHelper.Verify(password, user.HashPassword);

                if (!passwordValid)
                {
                    await WritePendingPageAsync(context, dbState, "Tài khoản hoặc mật khẩu không đúng.");
                    return;
                }

                var hasPermission = await db.Permissions
                    .Where(p => p.Code == MigratePermission)
                    .Where(p =>
                        db.UserPermissions.Any(up =>
                            up.UserId == user.Id &&
                            up.PermissionId == p.Id &&
                            up.Status == UserPermissionStatus.Grant)
                        ||
                        (
                            !db.UserPermissions.Any(up =>
                                up.UserId == user.Id &&
                                up.PermissionId == p.Id &&
                                up.Status == UserPermissionStatus.Deny)
                            &&
                            db.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Join(db.RolePermissions,
                                    ur => ur.RoleId,
                                    rp => rp.RoleId,
                                    (ur, rp) => rp.PermissionId)
                                .Any(pid => pid == p.Id)
                        )
                    )
                    .AnyAsync();

                if (!hasPermission)
                {
                    await WritePendingPageAsync(context, dbState, "Tài khoản không có quyền thực hiện thao tác này.");
                    return;
                }

                await db.Database.MigrateAsync();
                dbState.ClearPending();
                context.Response.Redirect("/");
            }
            catch (Exception ex)
            {
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
                        .box{background:#fff;border-radius:16px;padding:2.5rem;text-align:center;max-width:480px;width:90%;box-shadow:0 4px 24px rgba(0,0,0,.08)}
                        h1{color:#1a1a2e;font-size:1.5rem;margin-bottom:.6rem}
                        p{color:#555;line-height:1.6;margin-bottom:.75rem}
                        code{display:block;font-size:.78rem;color:#888;margin-bottom:1.25rem;word-break:break-all;text-align:left;background:#f8f8f8;padding:.6rem .8rem;border-radius:6px}
                        a.btn-admin{display:inline-block;padding:.6rem 1.5rem;background:#4b49ac;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;margin-bottom:1.25rem}
                        a.btn-admin:hover{background:#3d3c8e}
                        .divider{display:flex;align-items:center;gap:10px;color:#bbb;font-size:.8rem;margin-bottom:1rem}
                        .divider::before,.divider::after{content:'';flex:1;border-top:1px solid #eee}
                        details summary{cursor:pointer;color:#999;font-size:.82rem;list-style:none;padding:.4rem 0;user-select:none}
                        details summary:hover{color:#666}
                        details[open] summary{color:#555;margin-bottom:.75rem}
                        .field{position:relative;margin-bottom:.6rem}
                        .field input{width:100%;padding:.6rem .9rem;border:1px solid #ddd;border-radius:8px;font-size:.95rem}
                        .field input:focus{outline:none;border-color:#4b49ac}
                        .field .eye{position:absolute;right:.75rem;top:50%;transform:translateY(-50%);background:none;border:none;cursor:pointer;color:#aaa;padding:0}
                        .field .eye:hover{color:#555}
                        button[type=submit]{width:100%;padding:.65rem;background:#dc3545;color:#fff;border:none;border-radius:8px;font-size:.95rem;cursor:pointer;font-weight:600}
                        button[type=submit]:hover{background:#b02a37}
                        .err{color:#d9534f;font-size:.85rem;margin-bottom:.75rem;text-align:left;background:#fff5f5;padding:.5rem .75rem;border-radius:6px}
                    </style>
                </head>
                <body>
                    <div class="box">
                        <h1>⚙️ Đang cập nhật hệ thống</h1>
                        <p>Cơ sở dữ liệu cần được cập nhật trước khi tiếp tục.</p>
                        <code>{{names}}</code>

                        <a href="/auth/login" class="btn-admin">🔐 Đăng nhập Admin</a>

                        <div class="divider">hoặc migrate trực tiếp</div>

                        <details>
                            <summary>▶ Migrate khẩn cấp (khi không đăng nhập được)</summary>
                            {{errorHtml}}
                            <form method="post" action="{{MigratePath}}">
                                <div class="field">
                                    <input type="text" name="username" placeholder="Tên đăng nhập" autocomplete="username" required>
                                </div>
                                <div class="field">
                                    <input type="password" id="mpwd" name="password" placeholder="Mật khẩu" autocomplete="current-password" required>
                                    <button type="button" class="eye" onclick="togglePwd()">👁</button>
                                </div>
                                <button type="submit">Chạy Migration</button>
                            </form>
                        </details>
                    </div>
                    <script>
                        function togglePwd() {
                            var i = document.getElementById('mpwd');
                            i.type = i.type === 'password' ? 'text' : 'password';
                        }
                    </script>
                </body>
                </html>
                """);
        }
    }
}
