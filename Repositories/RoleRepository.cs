using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
	public class RoleRepository : RepositoryBase<RoleEntity>, IRoleRepository
	{
		public RoleRepository(IUnitOfWork uow, IUserContext userContext) : base(uow, userContext)
		{
		}
	}
}
