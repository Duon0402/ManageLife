namespace ManageLife.Base
{
	public class UpdatePermissionAttribute : AuthorizeCustomAttribute
	{
		public UpdatePermissionAttribute() : base(PermissionConst.Update)
		{
		}
	}
}
