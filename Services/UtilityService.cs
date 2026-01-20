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
        private readonly IExceptionItemService _exceptionItemService;

        public UtilityService(AppDbContext context, IExceptionItemService service) : base(context)
        {
            _exceptionItemService = service;
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
                var supervisor = "Hoàng Đức Hưng";

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
