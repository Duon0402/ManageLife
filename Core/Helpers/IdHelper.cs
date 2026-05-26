namespace ManageLife.Core
{
    public static class IdHelper
    {
        public static string NewId() => Guid.NewGuid().ToString();
    }
}
