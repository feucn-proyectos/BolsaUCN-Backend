using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.Validators
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
            : base("La fecha debe ser en el futuro.") { }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value != null)
            {
                DateTime dateValue = (DateTime)value;
                if (dateValue < DateTime.UtcNow)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
