using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace ManageLife.Base
{
    public class AuthorizeCustomAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public AuthorizeCustomAttribute(string permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var permissionsJson = context.HttpContext.Session.GetString("Permissions");

            if (string.IsNullOrEmpty(permissionsJson))
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();

            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
