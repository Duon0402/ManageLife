namespace ManageLife.Base.AuthorizeBase
{
    public class DeletePermissionAttribute : AuthorizeCustomAttribute
    {
        public DeletePermissionAttribute() : base(PermissionType.Delete)
        {
        }
    }
}
