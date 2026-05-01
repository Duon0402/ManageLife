namespace ManageLife.Contexts
{
    public interface IUserContext
    {
        string? GetUserId();
        string? GetUserName();
        IEnumerable<string> GetUserRoles();
        bool HasRole(string role);
    }
}
