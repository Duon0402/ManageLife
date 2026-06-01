using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Services
{
    public class UtilityService : IUtilityService
    {
        private readonly IExceptionItemService _exceptionItemService;

        public UtilityService(IExceptionItemService service)
        {
            _exceptionItemService = service;
        }

        public Result<EmailDailyReportModel> GenerateEmailDailyReport(GenerateEmailDailyReportRequest request)
        {
            string msg;
            try
            {
                if (request.Validate() is { } err) return Result.Error<EmailDailyReportModel>(Result.DATA_INVALID.Code, err);

                var currentBusinessDay = request.CurrentBusinessDay.Date;
                var nextBusinessDay = request.NextBusinessDay.Date;

                if (nextBusinessDay <= currentBusinessDay)
                {
                    msg = "Ngày kế tiếp phải lớn hơn ngày hiện tại.";
                    return Result.Error<EmailDailyReportModel>(Result.DATA_INVALID.Code, msg);
                }

                var fullName = "Đặng Trường Dương";
                var employeeCode = "002740";
                var department = "Tổ Web - Phòng PTPM";
                var supervisor = "Nguyễn Văn Chiến";

                var subject = string.Format(
                    EmailDailyReportTemplate.Subject,
                    fullName,
                    currentBusinessDay.ToString("dd.MM.yyyy"),
                    nextBusinessDay.ToString("dd.MM.yyyy")
                );

                var body = string.Format(
                    EmailDailyReportTemplate.Body,
                    fullName,
                    employeeCode,
                    department,
                    supervisor,
                    currentBusinessDay.ToString("dd.MM.yyyy"),
                    nextBusinessDay.ToString("dd.MM.yyyy"),
                    request.TodayWorkResults,
                    request.PlannedWorkTomorrow,
                    request.Suggestions.IsNotEmpty() ? request.Suggestions : "- Không có"
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
