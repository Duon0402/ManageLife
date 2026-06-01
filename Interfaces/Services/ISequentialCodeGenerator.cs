namespace ManageLife.Interfaces
{
    public interface ISequentialCodeGenerator
    {
        Task<string> NextAsync(string category, CancellationToken ct = default);
    }
}
