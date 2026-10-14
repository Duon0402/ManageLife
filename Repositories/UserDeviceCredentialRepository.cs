using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
    public class UserDeviceCredentialRepository : RepositoryBase<UserDeviceCredentialEntity>, IUserDeviceCredentialRepository
    {
        public UserDeviceCredentialRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
        {
        }
    }
}
