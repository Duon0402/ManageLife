namespace ManageLife.Base
{
    public class Result<T> : Result
    {
        public T? Data { get; set; }

        public Result(string code, string msg, T? data = default, string? errorContent = null)
            : base(code, msg, errorContent)
        {
            Data = data;
        }

        public Result<T> Ok(T data)
        {
            return new Result<T>("00", "Ok", data);
        }

        public new Result<T> Error(string code, string message, string? errorContent = null)
        {
            return new Result<T>(code, message, default, errorContent);
        }

        public new Result<T> Exception(string message, Exception ex)
        {
            return new Result<T>("99", message, default, ex.ToString());
        }
    }
}
