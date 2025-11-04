using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Controllers.Admin;
using ManageLife.Interfaces;

namespace ManageLife.Helpers
{
    // TODO: Thêm phần kiêm trả để ẩn menu Item nếu yêu cầu permission
    public class MenuRegister : IMenuRegister
    {
        private readonly ICacheService _cache;

        public MenuRegister(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<List<MenuItem>> GetListMenuItemsAsync()
        {
            var cacheKeyItem = CacheKey.MenuItems();

            var cacheData = await _cache.TryGetValueAsync<List<MenuItem>>(cacheKeyItem.Key);
            if (cacheData != null)
                return cacheData;

            var listMenuItems = BuildMenuItems();

            await _cache.SetAsync(cacheKeyItem.Key, listMenuItems);

            return listMenuItems;
        }

        private static List<MenuItem> BuildMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem<DashboardController>("Dashboard", x => x.Index(), "fa-solid fa-house"),
                new MenuItem<CronJobController>("Cron Job", x => x.Index(), "fa-solid fa-calendar-days"),

                new MenuItem("Language", null, "fa-solid fa-language", new List<MenuItem>
                {
                    new MenuItem<LanguageController>("Language", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                    new MenuItem<TranslationController>("Translation", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                }),
            };
        }
    }
}
