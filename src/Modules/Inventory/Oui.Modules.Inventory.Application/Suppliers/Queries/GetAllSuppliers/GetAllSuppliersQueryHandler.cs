using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetAllSuppliers;

internal sealed class GetAllSuppliersQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetAllSuppliersQuery, List<SupplierListResponse>>
{
    public async Task<Result<List<SupplierListResponse>>> Handle(
        GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(sup =>
                sup.Name.ToLower().Contains(s) ||
                sup.Email.ToLower().Contains(s) ||
                sup.PhoneNumber.Contains(s) ||
                (sup.TaxNumber != null && sup.TaxNumber.Contains(s)) ||
                sup.Initial.ToLower().Contains(s));
        }

        var suppliers = await query
            .OrderBy(sup => sup.Name)
            .Select(sup => new SupplierListResponse(
                sup.Id,
                sup.ExternalId,
                sup.Name,
                sup.Email,
                sup.PhoneNumber,
                sup.TaxNumber,
                sup.Initial,
                sup.CreditPercentageInStore,
                sup.CashRedemptionPercentage,
                sup.Items.Count(i => !i.IsDeleted),
                sup.CreatedOn))
            .ToListAsync(cancellationToken);

        return suppliers;
    }
}
