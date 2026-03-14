using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturns;

internal sealed class GetReturnsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReturnsQuery, SupplierReturnPagedResult>
{
    public async Task<Result<SupplierReturnPagedResult>> Handle(
        GetReturnsQuery request, CancellationToken cancellationToken)
    {
        var query = db.SupplierReturns
            .Include(r => r.Supplier)
            .AsQueryable();

        if (request.SupplierExternalId.HasValue)
        {
            var supplier = await db.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ExternalId == request.SupplierExternalId.Value, cancellationToken);

            if (supplier is not null)
                query = query.Where(r => r.SupplierId == supplier.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(r => r.Supplier.Name.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new SupplierReturnListItemResponse(
                r.ExternalId,
                new ReturnSupplierInfo(r.Supplier.ExternalId, r.Supplier.Name, r.Supplier.Initial),
                r.ReturnDate,
                r.ItemCount,
                r.Notes,
                r.CreatedOn))
            .ToListAsync(cancellationToken);

        return new SupplierReturnPagedResult(
            returns,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
