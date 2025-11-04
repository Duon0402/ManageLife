using ManageLife.Base;

namespace ManageLife.Helpers
{
	public interface IMenuRegister
	{
		public Task<List<MenuItem>> GetListMenuItemsAsync();
	}
}
