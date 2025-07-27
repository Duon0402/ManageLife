namespace ManageLife.Base.AuthorizeBase
{
	public class AccessPagePermissionAttribute : AuthorizeCustomAttribute
	{
		public AccessPagePermissionAttribute() : base(PermissionType.AccessPage)
		{
		}
	}
}
