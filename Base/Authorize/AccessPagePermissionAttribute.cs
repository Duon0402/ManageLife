namespace ManageLife.Base
{
	public class AccessPagePermissionAttribute : AuthorizeCustomAttribute
	{
		public AccessPagePermissionAttribute() : base(PermissionType.AccessPage)
		{
		}
	}
}
