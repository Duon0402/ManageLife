namespace ManageLife.Base.OptionModelBase
{
    public class ValidationOptions
    {
        public ValidationOptions(ValidationRuleType type, string msg)
        {
            RuleType = type;
            ErrorMessage = msg;
        }

        public ValidationOptions(string msg, string regexPattern)
        {
            RuleType = ValidationRuleType.Regex;
            ErrorMessage = msg;
            RegexPattern = regexPattern;
        }

        public ValidationRuleType RuleType { get; set; } = ValidationRuleType.None;
        public string? ErrorMessage { get; set; }
        public string? RegexPattern { get; set; }
    }

    //TODO: Thêm các kiểu validate khác
    public enum ValidationRuleType
    {
        None,
        Required,
        Email,
        Regex,
    }
}
