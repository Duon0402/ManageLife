using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class ExceptionItemRepository : RepositoryBase<ExceptionItemEntity>
    {
        public ExceptionItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
