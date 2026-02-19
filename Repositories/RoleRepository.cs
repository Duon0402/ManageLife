using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
	public class RoleRepository : RepositoryBase<RoleEntity>, IRoleRepository
	{
		public RoleRepository(IUnitOfWork uow) : base(uow)
		{
		}
	}
}
