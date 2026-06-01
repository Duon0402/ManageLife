using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Extensions
{
    public static class ValidationExtensions
    {
        public static string? Validate(this IValidatableRequest request)
        {
            var context = new ValidationContext(request, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(request, context, results, validateAllProperties: true);
            return results.FirstOrDefault()?.ErrorMessage;
        }
    }
}
