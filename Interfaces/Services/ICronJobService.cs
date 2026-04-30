using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ICronJobService
    {
        Task<Result<List<CronJobModel>>> GetListCronJobsAsync();
    }
}
