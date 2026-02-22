using ManageLife.Base;
using ManageLife.Interfaces;
using System.Threading.Channels;

namespace ManageLife.Services
{
    public class TelegramUploadQueue : ITelegramUploadQueue
    {
        private readonly Channel<string> _channel;
        public TelegramUploadQueue()
        {
            _channel = Channel.CreateUnbounded<string>(
                new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });
        }

        public async ValueTask EnqueueAsync(string fileId)
        {
            if (fileId.IsEmpty()) throw new ArgumentException(null, nameof(fileId));
            await _channel.Writer.WriteAsync(fileId);
        }

        public async ValueTask<string> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
