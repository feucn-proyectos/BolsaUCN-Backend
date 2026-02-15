using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend.src.Application.Validators
{
    public class PhoneValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            var phoneNumber = value as string;
            var regex = new Regex(@"^\+56\d{9}$");

            if (phoneNumber != null && !regex.IsMatch(phoneNumber))
            {
                return new ValidationResult("El número de teléfono no es válido.");
            }
            return ValidationResult.Success;
        }
    }
}
