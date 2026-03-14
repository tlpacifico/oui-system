using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierReceptions;

internal sealed class GetSupplierReceptionsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetSupplierReceptionsQuery, List<SupplierReceptionResponse>>
{
    public async Task<Result<List<SupplierReceptionResponse>>> Handle(
        GetSupplierReceptionsQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure<List<SupplierReceptionResponse>>(SupplierErrors.NotFound);

        var receptions = await db.Receptions
            .Where(r => r.SupplierId == supplier.Id)
            .OrderByDescending(r => r.ReceptionDate)
            .Select(r => new SupplierReceptionResponse(
                r.ExternalId,
                r.ReceptionDate,
                r.ItemCount,
                r.Status.ToString(),
                r.Items.Count(i => !i.IsDeleted),
                r.Items.Count(i => !i.IsDeleted && i.Status == ItemStatus.Evaluated),
                r.Items.Count(i => !i.IsDeleted && i.IsRejected),
                r.Notes,
                r.CreatedOn))
            .ToListAsync(cancellationToken);

        return receptions;
    }
}
