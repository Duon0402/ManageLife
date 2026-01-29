using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Controllers.Admin;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    // TODO: Thêm phần kiêm trả để ẩn menu Item nếu yêu cầu permission
    public class MenuRegister : IMenuRegister
    {
        private readonly ICacheService _cache;
        private readonly IPermissionService _permissionService;

        public MenuRegister(ICacheService cache, IPermissionService permissionService)
        {
            _cache = cache;
            _permissionService = permissionService;
        }

        public async Task<List<MenuItem>> GetListMenuItemsAsync()
        {
            var cacheItem = CacheItems.MenuItems();
            var baseMenu = await _cache.TryGetValueAsync<List<MenuItem>>(cacheItem);
            if (baseMenu == null)
            {
                baseMenu = BuildMenuItems();
                await _cache.SetAsync(baseMenu, cacheItem);
            }

            return await FilterMenuByPermissionAsync(baseMenu);
        }

        private List<MenuItem> BuildMenuItems()
        {
            return new List<MenuItem>
            {
                new MenuItem<DashboardController>("Dashboard", x => x.Index(), "fa-solid fa-house"),

                new MenuItem<CronJobController>("Cron Job", x => x.Index(), "fa-solid fa-calendar-days"),

                new MenuItem("Language", "fa-solid fa-language", new List<MenuItem>
                {
                    new MenuItem<LanguageController>("Language", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                    new MenuItem<TranslationController>("Translation", x => x.Index(), "fa-regular fa-circle fa-2xs"),
                }),
                new MenuItem("User Management", "fa-solid fa-user",new List<MenuItem>
                {
                    new MenuItem<PermissionController>("Permission", x => x.Index(), "fa-solid fa-key")
                })
            };
        }

        private async Task<List<MenuItem>> FilterMenuByPermissionAsync(List<MenuItem> allMenuItems)
        {
            var userId = GlobalHttpContext.GetUserId();

            HashSet<string> userPermissions = new(StringComparer.OrdinalIgnoreCase);

            if (!userId.IsEmpty())
            {
                var req = new GetAssignedPermissionsByUserIdRequest { UserId = userId };
                var res = await _permissionService.GetAssignedPermissionsByUserIdAsync(req);

                if (res.IsOk() && res.Data.IsNotEmpty())
                {
                    userPermissions = res.Data
                        .Select(x => x.Code)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }

            List<MenuItem> FilterRecursive(List<MenuItem> items)
            {
                var result = new List<MenuItem>();

                foreach (var item in items)
                {
                    bool hasPermission =
                        item.PermissionCode.IsEmpty() ||
                        userPermissions.Contains(item.PermissionCode);

                    if (item.HasSubItems)
                    {
                        var filteredSub = FilterRecursive(item.SubItems);

                        if (filteredSub.Count > 0 || hasPermission)
                        {
                            item.SubItems = filteredSub;
                            result.Add(item);
                        }
                    }
                    else if (hasPermission)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }

            return FilterRecursive(allMenuItems);
        }
    }
}
