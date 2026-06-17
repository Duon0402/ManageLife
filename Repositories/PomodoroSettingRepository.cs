using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class PomodoroSettingRepository : RepositoryBase<PomodoroSettingEntity>, IPomodoroSettingRepository
    {
        public PomodoroSettingRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
