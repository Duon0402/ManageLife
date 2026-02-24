using ManageLife.Base;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class AlbumRepository : RepositoryBase<AlbumEntity>, IAlbumRepository
    {
        public AlbumRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
