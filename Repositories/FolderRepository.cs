using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class FolderRepository : RepositoryBase<FolderEntity>, IFolderRepository
    {
        public FolderRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
