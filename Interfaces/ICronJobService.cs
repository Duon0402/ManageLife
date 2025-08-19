using ManageLife.Base;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ICronJobService
    {
        public Task<Result<List<CronJobModel>>> GetListCronJobsAsync();
    }
}
