using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManageLife.Base
{
    public class AuthorizeCustomAttribute : ActionFilterAttribute
    {
        private readonly PermissionType _permission;

        public AuthorizeCustomAttribute(PermissionType permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var permissions = context.HttpContext.Session.GetString("Permissions");
            if(string.IsNullOrEmpty(permissions) || !permissions.Contains(_permission.ToString()))
            {
                //TODO: Thêm page 403
                context.Result = new ForbidResult();
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
