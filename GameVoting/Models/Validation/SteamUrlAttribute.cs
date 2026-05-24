using System.ComponentModel.DataAnnotations;

namespace GameVoting.Models.Validation;

public class StoreUrlAttribute : ValidationAttribute
{
    private static readonly string[] AllowedDomains =
    [
        "store.steampowered.com",
        "epicgames.com",
        "gog.com",
        "microsoft.com",
        "xbox.com",
        "nintendo.com",
        "playstation.com",
        "ubisoft.com",
        "ea.com",
        "battle.net",
        "itch.io"
    ];

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null or "") return ValidationResult.Success;

        if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out Uri? uri))
            return new ValidationResult("URL inválida.");

        if (!AllowedDomains.Any(d => uri.Host.EndsWith(d)))
            return new ValidationResult($"URL deve ser de uma loja permitida.");

        return ValidationResult.Success;
    }
}
