namespace ManageLife.Base
{
    public static class IdHeper
    {
        public static string NewId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
