using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class PomodoroSessionRepository : RepositoryBase<PomodoroSessionEntity>, IPomodoroSessionRepository
    {
        public PomodoroSessionRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
