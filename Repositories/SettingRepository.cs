using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class SettingRepository : RepositoryBase<SettingEntity>
    {
        public SettingRepository(AppDbContext context) : base(context)
        {
        }
    }
}
