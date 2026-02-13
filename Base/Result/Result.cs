namespace ManageLife.Base
{
    public class Result
    {
        public string Code { get; }
        public string Message { get; }
        public string? ErrorContent { get; }

        public Result()
        {
            Code = "00";
            Message = "Ok";
        }

        public Result(string code, string message, string? errorContent = null)
        {
            Code = code;
            Message = message;
            ErrorContent = errorContent;
        }

        public bool IsOk() => Code == "00";

        public bool IsException() => Code == "99";

        public bool IsError() => !IsOk() && !IsException();

        public static Result Ok() => new("00", "Ok");

        public static Result Error(string code, string message, string? errorContent = null)
            => new(code, message, errorContent);

        public static Result Exception(string message, Exception ex)
            => new("99", message, ex.ToString());

        public static Result<T> Ok<T>(T data) => new("00", "Ok", data);

        public static Result<T> Error<T>(string code, string message, string? errorContent = null)
            => new(code, message, default!, errorContent);

        public static Result<T> Exception<T>(string message, Exception ex)
            => new("99", message, default!, ex.ToString());

        public static readonly Result DATA_NOT_CREATE = new("01", "Tạo dữ liệu thất bại");
        public static readonly Result DATA_NOT_UPDATE = new("02", "Cập nhật dữ liệu thất bại");
        public static readonly Result DATA_NOT_DELETE = new("03", "Xóa dữ liệu thất bại");
        public static readonly Result DATA_EXISTED = new("04", "Dữ liệu đã tồn tại");
        public static readonly Result DATA_NOT_EXISTED = new("05", "Dữ liệu không tồn tại");
        public static readonly Result DATA_VALID = new("06", "Dữ liệu hợp lệ");
        public static readonly Result DATA_INVALID = new("07", "Dữ liệu không hợp lệ");
        // TODO: Bổ sung các common
    }

    public class Result<T> : Result
    {
        public T Data { get; }

        public Result() : base()
        {
            Data = default!;
        }

        public Result(string code, string message, T data = default!, string? errorContent = null)
            : base(code, message, errorContent)
        {
            Data = data;
        }

        public T GetOrDefault(T defaultValue = default!) => Data is null ? defaultValue : Data;
    }
}
