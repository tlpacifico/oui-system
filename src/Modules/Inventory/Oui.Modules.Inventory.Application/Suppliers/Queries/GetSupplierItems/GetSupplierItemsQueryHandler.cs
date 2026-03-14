using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Application.Items;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierItems;

internal sealed class GetSupplierItemsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetSupplierItemsQuery, PagedResult<SupplierItemResponse>>
{
    public async Task<Result<PagedResult<SupplierItemResponse>>> Handle(
        GetSupplierItemsQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure<PagedResult<SupplierItemResponse>>(SupplierErrors.NotFound);

        var query = db.Items
            .Where(i => i.SupplierId == supplier.Id)
            .Include(i => i.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ItemStatus>(request.Status, out var itemStatus))
            query = query.Where(i => i.Status == itemStatus);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedOn)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new SupplierItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.Condition.ToString(),
                i.DaysInStock,
                i.CreatedOn))
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplierItemResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
