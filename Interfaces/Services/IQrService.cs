using ManageLife.Core;

namespace ManageLife.Interfaces
{
    public interface IQrService
    {
        Result<byte[]> GeneratePng(string text, int pixels = 20);
    }
}
