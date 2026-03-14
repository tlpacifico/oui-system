using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetPosSuppliers;

internal sealed class GetPosSuppliersQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetPosSuppliersQuery, List<PosSupplierResponse>>
{
    public async Task<Result<List<PosSupplierResponse>>> Handle(
        GetPosSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Suppliers.Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(sup =>
                sup.Name.ToLower().Contains(s) ||
                sup.Initial.ToLower().Contains(s));
        }

        var suppliers = await query
            .OrderBy(sup => sup.Name)
            .Select(sup => new PosSupplierResponse(sup.Id, sup.Name, sup.Initial))
            .Take(200)
            .ToListAsync(cancellationToken);

        return suppliers;
    }
}
