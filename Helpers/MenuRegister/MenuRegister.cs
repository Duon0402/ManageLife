using ManageLife.Base;

namespace ManageLife.Helpers
{
    public class MenuRegister : IMenuRegister
    {
        public List<MenuItem> GetListMenuItems()
        {
            var listMenuItems = new List<MenuItem>
            {
                new MenuItem("Dashboard", "/Admin/Dashboard", "fa-solid fa-house"),
                new MenuItem("Cron Job", "/Admin/CronJob", "fa-solid fa-calendar-days"),
            };

            return listMenuItems;
        }
    }
}
