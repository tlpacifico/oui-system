using FluentValidation;

namespace Oui.Modules.Inventory.Application.Colors.Commands.CreateColor;

internal sealed class CreateColorCommandValidator : AbstractValidator<CreateColorCommand>
{
    public CreateColorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.HexCode)
            .Must(BeValidHexColorOrNull)
            .WithMessage("HexCode must be a valid hex color (e.g. #FF5733).");
    }

    private static bool BeValidHexColorOrNull(string? color)
    {
        if (color is null) return true;
        color = color.Trim();
        if (!color.StartsWith('#')) return false;
        if (color.Length != 7) return false;
        return color[1..].All(c => "0123456789abcdefABCDEF".Contains(c));
    }
}
