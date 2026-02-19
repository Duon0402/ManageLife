using ManageLife.Base;
using ManageLife.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManageLife.Controllers.Client
{
    public class QrController : WebClientControllerBase
    {
        private readonly IQrService _service;

        public QrController(IQrService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Generate(string text)
        {
            var result = _service.GeneratePng(text, 30);

            if (!result.IsOk())
                return Json(result);

            var bytes = result.Data;
            if (bytes == null || bytes.Length == 0)
                return Json(Result.Error("07", "QR generation failed"));

            var base64 = Convert.ToBase64String(bytes);

            return Json(Result.Ok(base64));
        }
    }
}
