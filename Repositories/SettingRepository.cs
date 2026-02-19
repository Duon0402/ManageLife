using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class SettingRepository : RepositoryBase<SettingEntity>, ISettingRepository
    {
        public SettingRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
