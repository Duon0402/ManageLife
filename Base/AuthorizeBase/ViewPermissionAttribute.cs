namespace ManageLife.Base
{
    public class ViewPermissionAttribute : AuthorizeCustomAttribute
    {
        public ViewPermissionAttribute() : base(PermissionType.View)
        {
        }
    }
}
