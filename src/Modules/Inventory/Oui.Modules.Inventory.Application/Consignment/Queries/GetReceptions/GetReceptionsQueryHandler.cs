using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptions;

internal sealed class GetReceptionsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReceptionsQuery, ReceptionPagedResult>
{
    public async Task<Result<ReceptionPagedResult>> Handle(
        GetReceptionsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Receptions
            .Include(r => r.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ReceptionStatus>(request.Status, out var receptionStatus))
            query = query.Where(r => r.Status == receptionStatus);

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

        var receptions = await query
            .OrderByDescending(r => r.ReceptionDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReceptionListItemResponse(
                r.ExternalId,
                new ReceptionSupplierInfo(r.Supplier.ExternalId, r.Supplier.Name, r.Supplier.Initial),
                r.ReceptionDate,
                r.ItemCount,
                r.Status.ToString(),
                r.Items.Count(i => !i.IsDeleted),
                r.Items.Count(i => !i.IsDeleted && !i.IsRejected && i.Status != ItemStatus.Rejected),
                r.Items.Count(i => !i.IsDeleted && i.IsRejected),
                r.Notes,
                r.CreatedOn))
            .ToListAsync(cancellationToken);

        return new ReceptionPagedResult(
            receptions,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
