using ManageLife.Commons;
using ManageLife.Interfaces;

namespace ManageLife.Helpers
{
    public class ShortUrlCodeGenerator(ISequentialCodeGenerator generator)
    {
        public Task<string> NextAsync(CancellationToken ct = default)
            => generator.NextAsync(CodeSequenceCategory.ShortUrl, ct);
    }
}
