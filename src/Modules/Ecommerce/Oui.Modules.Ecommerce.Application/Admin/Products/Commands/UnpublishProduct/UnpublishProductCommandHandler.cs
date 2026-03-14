using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UnpublishProduct;

internal sealed class UnpublishProductCommandHandler(EcommerceDbContext db)
    : ICommandHandler<UnpublishProductCommand, UnpublishProductResponse>
{
    public async Task<Result<UnpublishProductResponse>> Handle(
        UnpublishProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (product is null)
            return Result.Failure<UnpublishProductResponse>(ProductErrors.NotFound);

        if (product.Status == EcommerceProductStatus.Unpublished)
            return Result.Failure<UnpublishProductResponse>(ProductErrors.AlreadyUnpublished);

        product.Status = EcommerceProductStatus.Unpublished;
        product.UnpublishedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new UnpublishProductResponse(product.ExternalId, "Produto despublicado com sucesso.");
    }
}
