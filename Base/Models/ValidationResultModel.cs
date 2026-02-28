namespace ManageLife.Base
{
    public class ValidationResultModel
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ValidationResultModel Success() => new() { IsValid = true };
        public static ValidationResultModel Fail(IEnumerable<string> errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }
}
