using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands.Commands.DeleteBrand;

internal sealed class DeleteBrandCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteBrandCommand>
{
    public async Task<Result> Handle(
        DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await db.Brands
            .FirstOrDefaultAsync(b => b.ExternalId == request.ExternalId, cancellationToken);

        if (brand is null)
            return Result.Failure(BrandErrors.NotFound);

        var hasItems = await db.Items.AnyAsync(i => i.BrandId == brand.Id && !i.IsDeleted, cancellationToken);
        if (hasItems)
            return Result.Failure(BrandErrors.HasItems);

        brand.IsDeleted = true;
        brand.DeletedBy = "system";
        brand.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
