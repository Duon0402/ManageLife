using ManageLife.Core;

namespace ManageLife.Services
{
	public interface IMenuRegister
	{
		public Task<List<MenuItem>> GetListMenuItemsAsync();
	}
}
