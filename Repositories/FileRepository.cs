using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
    public class FileRepository : RepositoryBase<FileEntity>
    {
        public FileRepository(AppDbContext context) : base(context)
        {
        }
    }
}
