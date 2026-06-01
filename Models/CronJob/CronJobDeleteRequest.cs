using ManageLife.Core;

namespace ManageLife.Models
{
    public class CronJobDeleteRequest : IValidatableRequest
    {
        public int JobId { get; set; }
    }
}
