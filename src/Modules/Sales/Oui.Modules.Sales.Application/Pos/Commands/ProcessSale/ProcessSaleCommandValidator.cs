using FluentValidation;

namespace Oui.Modules.Sales.Application.Pos.Commands.ProcessSale;

internal sealed class ProcessSaleCommandValidator : AbstractValidator<ProcessSaleCommand>
{
    public ProcessSaleCommandValidator()
    {
        RuleFor(x => x.CashRegisterId)
            .NotEmpty().WithMessage("Cash register ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("A venda deve ter pelo menos um item.");

        RuleFor(x => x.Payments)
            .NotEmpty().WithMessage("A venda deve ter pelo menos um pagamento.");

        RuleFor(x => x.Payments)
            .Must(p => p is null || p.Length <= 2)
            .WithMessage("Máximo de 2 métodos de pagamento por venda.");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.DiscountPercentage.HasValue)
            .WithMessage("A percentagem de desconto deve estar entre 0 e 100.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
