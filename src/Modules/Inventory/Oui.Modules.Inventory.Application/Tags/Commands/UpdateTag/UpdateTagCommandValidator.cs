using FluentValidation;

namespace Oui.Modules.Inventory.Application.Tags.Commands.UpdateTag;

internal sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Color)
            .Must(BeValidHexColorOrNull)
            .WithMessage("Color must be a valid hex color (e.g. #FF5733).");
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
