using ManageLife.Base;

namespace ManageLife.Helpers
{
    public class MenuRegister : IMenuRegister
    {
        public List<MenuItem> GetListMenuItems()
        {
            var listMenuItems = new List<MenuItem>
            {
                new MenuItem("Dashboard", "/Admin/Dashboard", "fa-solid fa-house")
            };

            return listMenuItems;
        }
    }
}
