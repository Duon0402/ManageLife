using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IUtilityService
    {
        Result<EmailDailyReportModel> GenerateEmailDailyReport(GenerateEmailDailyReportRequest request);
    }
}
