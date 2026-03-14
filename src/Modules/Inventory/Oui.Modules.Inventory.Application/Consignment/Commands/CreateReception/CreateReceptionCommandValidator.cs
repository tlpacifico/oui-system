using FluentValidation;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.CreateReception;

internal sealed class CreateReceptionCommandValidator : AbstractValidator<CreateReceptionCommand>
{
    public CreateReceptionCommandValidator()
    {
        RuleFor(x => x.SupplierExternalId)
            .NotNull().WithMessage("O fornecedor é obrigatório.");

        RuleFor(x => x.ItemCount)
            .GreaterThan(0).WithMessage("A quantidade de itens deve ser maior que zero.")
            .LessThanOrEqualTo(500).WithMessage("A quantidade máxima por recepção é de 500 itens.");
    }
}
