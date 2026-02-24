using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class AlbumFileRepository : RepositoryBase<AlbumFileEntity>, IAlbumFileRepository
    {
        public AlbumFileRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
