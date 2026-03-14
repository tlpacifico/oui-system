using FluentValidation;

namespace Oui.Modules.Sales.Application.Pos.Commands.CloseRegister;

internal sealed class CloseRegisterCommandValidator : AbstractValidator<CloseRegisterCommand>
{
    public CloseRegisterCommandValidator()
    {
        RuleFor(x => x.RegisterExternalId)
            .NotEmpty().WithMessage("Register ID is required.");

        RuleFor(x => x.ClosingAmount)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de fecho não pode ser negativo.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
