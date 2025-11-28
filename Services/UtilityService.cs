using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class UtilityService : ServiceBase, IUtilityService
    {
        public UtilityService(AppDbContext context) : base(context)
        {
        }

        public Result<EmailDailyReportModel> GenerateEmailDailyReport(GenerateEmailDailyReportRequest request)
        {
            string msg;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error<EmailDailyReportModel>(Result.DATA_INVALID.Code, msg);
                }

                var today = DateTimeHelper.VNTime().Date;

                var nextBusinessDate = today.DayOfWeek == DayOfWeek.Saturday
                    ? today.AddDays(2)
                    : today.AddDays(1);

                var fullName = "Đặng Trường Dương";
                var employeeCode = "002740";
                var department = "Tổ Web - Phòng PTPM";
                var supervisor = "Hoàng Đức Hưng";

                var subject = string.Format(
                    EmailDailyReportTemplate.Subject,
                    fullName,
                    today.ToString("dd.MM.yyyy"),
                    nextBusinessDate.ToString("dd.MM.yyyy")
                );

                var body = string.Format(
                    EmailDailyReportTemplate.Body,
                    fullName,
                    employeeCode,
                    department,
                    supervisor,
                    today.ToString("dd.MM.yyyy"),
                    nextBusinessDate.ToString("dd.MM.yyyy"),
                    request.TodayResult,
                    request.TomorrowPlan,
                    request.Suggestion.IsNotEmpty() ? request.Suggestion : "- Không có"
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
