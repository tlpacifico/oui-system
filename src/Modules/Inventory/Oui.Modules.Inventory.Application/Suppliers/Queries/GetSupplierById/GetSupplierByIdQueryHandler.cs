using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierById;

internal sealed class GetSupplierByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetSupplierByIdQuery, SupplierDetailResponse>
{
    public async Task<Result<SupplierDetailResponse>> Handle(
        GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .Where(sup => sup.ExternalId == request.ExternalId)
            .Select(sup => new SupplierDetailResponse(
                sup.Id,
                sup.ExternalId,
                sup.Name,
                sup.Email,
                sup.PhoneNumber,
                sup.TaxNumber,
                sup.Initial,
                sup.Notes,
                sup.CreditPercentageInStore,
                sup.CashRedemptionPercentage,
                sup.Items.Count(i => !i.IsDeleted),
                sup.CreatedOn,
                sup.CreatedBy,
                sup.UpdatedOn,
                sup.UpdatedBy))
            .FirstOrDefaultAsync(cancellationToken);

        return supplier is null
            ? Result.Failure<SupplierDetailResponse>(SupplierErrors.NotFound)
            : supplier;
    }
}
