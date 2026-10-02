using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManageLife.Core
{
    /// <summary>
    /// Xoá ErrorContent (stack trace của Result.Exception) khỏi response JSON để không lộ
    /// chi tiết nội bộ ra client. Exception đã được log ở service trước khi trả Result.
    /// Chỉ đăng ký ngoài môi trường Development (xem Program.cs).
    /// </summary>
    public class HideErrorContentFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var value = context.Result switch
            {
                ObjectResult objectResult => objectResult.Value,
                JsonResult jsonResult => jsonResult.Value,
                _ => null
            };

            if (value is Result { ErrorContent: not null } result)
                result.HideErrorContent();
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
