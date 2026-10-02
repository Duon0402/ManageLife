using ManageLife.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ManageLife.Core
{
    /// <summary>
    /// Chỉ cho phép request mang header X-Cron-Secret khớp CronJob:WebhookSecret.
    /// Dùng cho các endpoint chỉ cron job (cron-job.org) được gọi.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CronSecretAttribute : Attribute, IAuthorizationFilter
    {
        public const string HeaderName = "X-Cron-Secret";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<CronJobOptions>>();
            var secret = context.HttpContext.Request.Headers[HeaderName].ToString();

            if (!IsValidSecret(secret, options.Value.WebhookSecret))
                context.Result = new UnauthorizedResult();
        }

        private static bool IsValidSecret(string secret, string? expected)
        {
            //NOTE: Chưa cấu hình secret thì chặn hết, tránh mở endpoint khi thiếu config
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrEmpty(secret))
                return false;

            //NOTE: So sánh constant-time để không lộ secret qua thời gian phản hồi
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(expected));
        }
    }
}
