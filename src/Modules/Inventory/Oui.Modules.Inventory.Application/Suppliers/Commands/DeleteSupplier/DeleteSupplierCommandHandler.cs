using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.DeleteSupplier;

internal sealed class DeleteSupplierCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteSupplierCommand>
{
    public async Task<Result> Handle(
        DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        var hasItems = await db.Items.AnyAsync(i => i.SupplierId == supplier.Id && !i.IsDeleted, cancellationToken);
        if (hasItems)
            return Result.Failure(SupplierErrors.HasItems);

        var hasReceptions = await db.Receptions.AnyAsync(r => r.SupplierId == supplier.Id && !r.IsDeleted, cancellationToken);
        if (hasReceptions)
            return Result.Failure(SupplierErrors.HasReceptions);

        supplier.IsDeleted = true;
        supplier.DeletedBy = "system";
        supplier.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
