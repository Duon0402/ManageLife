using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FolderFileRepository : RepositoryBase<FolderFileEntity>, IFolderFileRepository
    {
        public FolderFileRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
