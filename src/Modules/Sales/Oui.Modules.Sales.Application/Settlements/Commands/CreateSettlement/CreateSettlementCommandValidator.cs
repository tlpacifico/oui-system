using FluentValidation;

namespace Oui.Modules.Sales.Application.Settlements.Commands.CreateSettlement;

internal sealed class CreateSettlementCommandValidator : AbstractValidator<CreateSettlementCommand>
{
    public CreateSettlementCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID is required.");

        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("Period end must be after period start.");
    }
}
