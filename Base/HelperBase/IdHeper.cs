namespace ManageLife.Base
{
    public static class IdHeper
    {
        public static string NewId()
        {
            return new Guid().ToString();
        }
    }
}
