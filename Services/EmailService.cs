using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using System.Net;
using System.Net.Mail;

namespace ManageLife.Services
{
    public class EmailService : ServiceBase<EmailService>, IEmailService
    {
        private readonly ISettingContext _settingContext;

        public EmailService(
            ISettingContext settingContext,
            IAppLogger<EmailService> logger,
            IUserContext userContext) : base(logger, userContext)
        {
            _settingContext = settingContext;
        }

        public async Task<Result> SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default)
        {
            try
            {
                var smtpHost = await _settingContext.GetStringAsync(SettingKeys.Email.SmtpHost);
                if (smtpHost.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Chưa cấu hình SMTP");

                var mailFrom = await _settingContext.GetStringAsync(SettingKeys.Email.MailFrom);
                if (mailFrom.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Chưa cấu hình địa chỉ email gửi đi (MailFrom)");

                var smtpPort = await _settingContext.GetIntAsync(SettingKeys.Email.SmtpPort, 587);
                var smtpUsername = await _settingContext.GetStringAsync(SettingKeys.Email.SmtpUsername);
                var smtpPassword = await _settingContext.GetStringAsync(SettingKeys.Email.SmtpPassword);
                var smtpEnableSsl = await _settingContext.GetBoolAsync(SettingKeys.Email.SmtpEnableSsl, true);
                var mailFromName = await _settingContext.GetStringAsync(SettingKeys.Email.MailFromName);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = smtpEnableSsl,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(mailFrom ?? string.Empty, mailFromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);

                await client.SendMailAsync(message, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi email");
                return Result.Exception("Lỗi khi gửi email", ex);
            }
        }
    }
}
