using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class UtilityService : ServiceBase, IUtilityService
    {
        public UtilityService(AppDbContext context) : base(context)
        {
        }

        public Result<EmailDailyReportModel> GenerateEmailDailyReport()
        {
            string msg;
            try
            {
                var today = DateTimeHelper.VNTime().Date;
                var nextDay = today.AddDays(1);
                var fullName = "Đặng Trường Dương";
                var employeeCode = "002740";
                var department = "Tổ Web - Phòng PTPM";

                var subject = string.Format(
                    EmailDailyReportTemplate.Subject,
                    fullName,
                    today.ToString("dd.MM.yyyy"),
                    nextDay.ToString("dd.MM.yyyy")
                );

                var body = string.Format(
                    EmailDailyReportTemplate.Body,
                    fullName,
                    employeeCode,
                    department,
                    today.ToString("dd.MM.yyyy"),
                    nextDay.ToString("dd.MM.yyyy")
                );

                var emailDailyReport = new EmailDailyReportModel
                {
                    EmailTo = EmailDailyReportReceiver.EmailTo,
                    EmailCc = EmailDailyReportReceiver.EmailCc,
                    Subject = subject,
                    Body = body
                };

                return Result.Ok(emailDailyReport);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<EmailDailyReportModel>(msg, ex);
            }
        }
    }
}
