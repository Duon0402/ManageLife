using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Entities;

namespace ManageLife.Repositories
{
	public class UserRepository : RepositoryBase<UserEntity>
	{
		public UserRepository(AppDbContext context) : base(context)
		{
		}
	}
}
