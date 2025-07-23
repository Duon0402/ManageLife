namespace ManageLife.Base
{
    public class Result
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string? ErrorContent { get; set; }

        public Result(string code, string message, string? errorContent = null)
        {
            Code = code;
            Message = message;
            ErrorContent = errorContent;
        }

        public static Result Ok()
        {
            return new Result("00", "Ok");
        }

        public static Result Error(string code, string message, string? errorContent = null)
        {
            return new Result(code, message, errorContent);
        }

        public static Result Exception(string msg, Exception ex)
        {
            return new Result("99", msg, ex.ToString());
        }

        public bool IsOk()
        {
            return Code == "00";
        }

        public static Result<T> Ok<T>(T data)
        {
            return new Result<T>("00", "Ok", data);
        }

        public static Result<T> Error<T>(string code, string message, string? errorContent = null)
        {
            return new Result<T>(code, message, default, errorContent);
        }

        public static Result<T> Exception<T>(string message, Exception ex)
        {
            return new Result<T>("99", message, default, ex.ToString());
        }

        public static readonly Result DATA_NOT_CREATE = new Result("01", "Dữ liệu không được thêm mới");
        public static readonly Result DATA_NOT_UPDATE = new Result("02", "Dữ liệu không được chỉnh sửa");
        public static readonly Result DATA_NOT_DELETE = new Result("03", "Dữ liệu không được xóa");
        public static readonly Result DATA_EXISTED = new Result("04", "Dữ liệu đã tồn tại");
        public static readonly Result DATA_NOT_EXISTED = new Result("05", "Dữ liệu không tồn tại");
        public static readonly Result DATA_VALID = new Result("06", "Dữ liệu hợp lệ");
        public static readonly Result DATA_INVALID = new Result("07", "Dữ liệu không hợp lệ");
    }
}
