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

        //TODO: Tối ưu lại code phần base này
        public Result<T> Ok<T>(T data)
        {
            return new Result<T>("00", "Ok", data);
        }

        public Result<T> Error<T>(string code, string message, string? errorContent = null)
        {
            return new Result<T>(code, message, default, errorContent);
        }

        public Result<T> Exception<T>(string message, Exception ex)
        {
            return new Result<T>("99", message, default, ex.ToString());
        }
    }
}
