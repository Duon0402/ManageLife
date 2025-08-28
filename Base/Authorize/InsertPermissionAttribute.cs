namespace ManageLife.Base
{
	public class InsertPermissionAttribute : AuthorizeCustomAttribute
	{
		public InsertPermissionAttribute() : base(PermissionType.Insert)
		{
		}
	}
}
