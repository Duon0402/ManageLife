using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ICronJobService
    {
        Task<Result<List<CronJobModel>>> GetListCronJobsAsync();
    }
}
