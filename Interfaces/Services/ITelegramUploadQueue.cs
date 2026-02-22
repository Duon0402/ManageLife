namespace ManageLife.Interfaces
{
    public interface ITelegramUploadQueue
    {
        ValueTask EnqueueAsync(string fileId);
        ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
    }
}
