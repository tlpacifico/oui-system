using FluentValidation;

namespace Oui.Modules.Sales.Application.CashRedemptions.Commands.ProcessCashRedemption;

internal sealed class ProcessCashRedemptionCommandValidator : AbstractValidator<ProcessCashRedemptionCommand>
{
    public ProcessCashRedemptionCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor deve ser positivo.");
    }
}
