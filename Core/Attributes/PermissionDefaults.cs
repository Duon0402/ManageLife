namespace ManageLife.Core
{
    public class ViewPermissionAttribute : PermissionAttribute
    {
        public ViewPermissionAttribute() : base(PermissionConst.View) { }
    }

    public class UpdatePermissionAttribute : PermissionAttribute
    {
        public UpdatePermissionAttribute() : base(PermissionConst.Update) { }
    }

    public class InsertPermissionAttribute : PermissionAttribute
    {
        public InsertPermissionAttribute() : base(PermissionConst.Insert) { }
    }

    public class DeletePermissionAttribute : PermissionAttribute
    {
        public DeletePermissionAttribute() : base(PermissionConst.Delete) { }
    }

    public class AccessPagePermissionAttribute : PermissionAttribute
    {
        public AccessPagePermissionAttribute() : base(PermissionConst.AccessPage) { }
    }
}
