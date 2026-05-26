using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class SettingRepository : RepositoryBase<SettingEntity>, ISettingRepository
    {
        public SettingRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
