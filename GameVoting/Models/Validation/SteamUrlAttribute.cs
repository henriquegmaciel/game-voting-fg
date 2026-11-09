using System.ComponentModel.DataAnnotations;

namespace GameVoting.Models.Validation;

public class StoreUrlAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null or "") return ValidationResult.Success;

        if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out Uri? uri))
            return new ValidationResult("URL inválida.");

        return ValidationResult.Success;
    }
}
