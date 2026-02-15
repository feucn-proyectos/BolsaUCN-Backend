using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.Validators
{
    public class CompareDateAttribute : ValidationAttribute
    {
        private readonly string _comparisonDateName;

        public CompareDateAttribute(string comparisonDateName)
        {
            _comparisonDateName = comparisonDateName;
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime currentValue = (DateTime)value;
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonDateName);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"La propiedad {_comparisonDateName} no existe.");
            }

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);

            DateTime comparisonDate = (DateTime)comparisonValue!;
            if (currentValue >= comparisonDate)
            {
                var errorMessage =
                    ErrorMessage
                    ?? $"{validationContext.DisplayName} debe ser anterior a {_comparisonDateName}.";
                return new ValidationResult(errorMessage, new[] { validationContext.MemberName! });
            }
            return ValidationResult.Success;
        }
    }
}
