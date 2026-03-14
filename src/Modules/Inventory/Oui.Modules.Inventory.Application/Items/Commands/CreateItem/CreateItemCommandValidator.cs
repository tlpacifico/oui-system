using FluentValidation;

namespace Oui.Modules.Inventory.Application.Items.Commands.CreateItem;

internal sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.");
        RuleFor(x => x.EvaluatedPrice).GreaterThan(0).WithMessage("O preço deve ser maior que zero.");
        RuleFor(x => x.Size).NotEmpty().WithMessage("O tamanho é obrigatório.");
        RuleFor(x => x.Color).NotEmpty().WithMessage("A cor é obrigatória.");
    }
}
