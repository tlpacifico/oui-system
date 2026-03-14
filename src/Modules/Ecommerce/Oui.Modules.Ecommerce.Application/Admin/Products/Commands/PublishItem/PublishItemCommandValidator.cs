using FluentValidation;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishItem;

internal sealed class PublishItemCommandValidator : AbstractValidator<PublishItemCommand>
{
    public PublishItemCommandValidator()
    {
        RuleFor(x => x.ItemExternalId)
            .NotEmpty().WithMessage("ItemExternalId is required.");
    }
}
