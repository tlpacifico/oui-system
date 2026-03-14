using FluentValidation;

namespace Oui.Modules.Sales.Application.Pos.Commands.OpenRegister;

internal sealed class OpenRegisterCommandValidator : AbstractValidator<OpenRegisterCommand>
{
    public OpenRegisterCommandValidator()
    {
        RuleFor(x => x.OpeningAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de abertura não pode ser negativo.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
