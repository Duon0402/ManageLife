namespace ManageLife.Base
{
	public class Result
	{
		public string Code { get; }
		public string Message { get; }
		public string? ErrorContent { get; }

		protected Result(string code, string message, string? errorContent = null)
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

		// CRUD
		public static readonly Result DATA_NOT_CREATE = new Result("01", "Dữ liệu không được thêm mới");
		public static readonly Result DATA_NOT_UPDATE = new Result("02", "Dữ liệu không được chỉnh sửa");
		public static readonly Result DATA_NOT_DELETE = new Result("03", "Dữ liệu không được xóa");
		public static readonly Result DATA_EXISTED = new Result("04", "Dữ liệu đã tồn tại");
		public static readonly Result DATA_NOT_EXISTED = new Result("05", "Dữ liệu không tồn tại");
		public static readonly Result DATA_VALID = new Result("06", "Dữ liệu hợp lệ");
		public static readonly Result DATA_INVALID = new Result("07", "Dữ liệu không hợp lệ");

		// TODO: Bổ sung các common
	}

	public class Result<T> : Result
	{
		public T Data { get; }

		internal Result(string code, string message, T data = default!, string? errorContent = null)
			: base(code, message, errorContent)
		{
			Data = data;
		}

		public T GetOrDefault(T defaultValue = default!) => Data is null ? defaultValue : Data;
	}
}
