using ManageLife.Interfaces;

namespace ManageLife.Services
{
    public class TelegramUploadWorker : BackgroundService
    {
        private readonly ITelegramUploadQueue _queue;
        private readonly IServiceProvider _provider;
        public TelegramUploadWorker(ITelegramUploadQueue queue, IServiceProvider provider)
        {
            _queue = queue;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var fileId = await _queue.DequeueAsync(stoppingToken);
                using var scope = _provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ITelegramFileService>();
                await service.UploadToTelegramAsync(fileId);
            }
        }
    }
}
