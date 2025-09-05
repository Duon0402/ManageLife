using ManageLife.Base;
using ManageLife.Commons;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Extentions
{
    //TODO: Cải tiến cho phần UI (validate form + bootstrap)
    public static class ValidationExtensions
    {
        public static ValidationResultModel Validate<T>(this T instance)
        {
            if (instance == null)
            {
                return ValidationResultModel.Fail(new[] { TranslationKey.Common.Message.DataInvalid });
            }

            var context = new ValidationContext(instance, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

            if (isValid)
                return ValidationResultModel.Success();

            return ValidationResultModel.Fail(results.Select(r => r.ErrorMessage ?? string.Empty));
        }
    }
}
