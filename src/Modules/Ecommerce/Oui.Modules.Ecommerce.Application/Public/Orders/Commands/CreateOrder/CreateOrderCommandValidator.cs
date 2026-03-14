using FluentValidation;

namespace Oui.Modules.Ecommerce.Application.Public.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Nome é obrigatório.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.ProductSlugs)
            .NotEmpty().WithMessage("Selecione pelo menos um produto.");
    }
}
