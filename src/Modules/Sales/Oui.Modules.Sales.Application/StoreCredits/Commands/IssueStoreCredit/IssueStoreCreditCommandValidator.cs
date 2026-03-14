using FluentValidation;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.IssueStoreCredit;

internal sealed class IssueStoreCreditCommandValidator : AbstractValidator<IssueStoreCreditCommand>
{
    public IssueStoreCreditCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
