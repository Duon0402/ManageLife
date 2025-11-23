using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Interfaces;
using QRCoder;

namespace ManageLife.Services
{
    public class QrService : ServiceBase, IQrService
    {
        public QrService(AppDbContext context) : base(context)
        {
        }

        public Result<byte[]> GeneratePng(string text, int pixels = 20)
        {
            string msg;
            try
            {
                if (text.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
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
                return Result.Exception<byte[]>(msg, ex);
            }
        }
    }
}
