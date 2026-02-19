using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FileRepository : RepositoryBase<FileEntity>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
