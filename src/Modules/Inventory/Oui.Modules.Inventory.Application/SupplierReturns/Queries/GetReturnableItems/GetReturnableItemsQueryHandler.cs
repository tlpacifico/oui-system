using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnableItems;

internal sealed class GetReturnableItemsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReturnableItemsQuery, List<ReturnableItemResponse>>
{
    public async Task<Result<List<ReturnableItemResponse>>> Handle(
        GetReturnableItemsQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == request.SupplierExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure<List<ReturnableItemResponse>>(SupplierReturnErrors.SupplierNotFound);

        var items = await db.Items
            .Where(i => i.SupplierId == supplier.Id
                        && !i.IsDeleted
                        && (i.Status == ItemStatus.ToSell || i.Status == ItemStatus.AwaitingAcceptance || i.Status == ItemStatus.Rejected))
            .Include(i => i.Brand)
            .Include(i => i.Photos.Where(p => p.IsPrimary))
            .OrderBy(i => i.IdentificationNumber)
            .Select(i => new ReturnableItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.IsRejected,
                i.DaysInStock,
                i.Photos.Where(p => p.IsPrimary).Select(p => p.FilePath).FirstOrDefault(),
                i.CreatedOn))
            .ToListAsync(cancellationToken);

        return items;
    }
}
