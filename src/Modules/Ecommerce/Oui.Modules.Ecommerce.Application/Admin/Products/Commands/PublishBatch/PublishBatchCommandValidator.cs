using FluentValidation;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishBatch;

internal sealed class PublishBatchCommandValidator : AbstractValidator<PublishBatchCommand>
{
    public PublishBatchCommandValidator()
    {
        RuleFor(x => x.ItemExternalIds)
            .NotEmpty().WithMessage("At least one item must be provided.");
    }
}
