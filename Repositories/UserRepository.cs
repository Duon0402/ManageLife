using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Interfaces;

namespace ManageLife.Repositories
{
	public class UserRepository : RepositoryBase<UserEntity>, IUserRepository
	{
		public UserRepository(IUnitOfWork uow) : base(uow)
		{
		}
	}
}
