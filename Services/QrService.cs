using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Interfaces;
using QRCoder;

namespace ManageLife.Services
{
    public class QrService : IQrService
    {
        private readonly IAppLogger<QrService> _logger;

        public QrService(IAppLogger<QrService> logger)
        {
            _logger = logger;
        }

        public Result<byte[]> GeneratePng(string text, int pixels = 20)
        {
            string msg;
            try
            {
                if (text.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<byte[]>(Result.DATA_INVALID.Code, msg);
                }

                using var generator = new QRCodeGenerator();
                var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

                var pngQr = new PngByteQRCode(data);
                return Result.Ok(pngQr.GetGraphic(pixels));
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<byte[]>(msg, ex);
            }
        }
    }
}
