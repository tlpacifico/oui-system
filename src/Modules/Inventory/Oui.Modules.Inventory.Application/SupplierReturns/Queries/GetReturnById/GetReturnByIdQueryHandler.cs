using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnById;

internal sealed class GetReturnByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReturnByIdQuery, SupplierReturnDetailResponse>
{
    public async Task<Result<SupplierReturnDetailResponse>> Handle(
        GetReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var supplierReturn = await db.SupplierReturns
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (supplierReturn is null)
            return Result.Failure<SupplierReturnDetailResponse>(SupplierReturnErrors.NotFound);

        return new SupplierReturnDetailResponse(
            supplierReturn.ExternalId,
            new ReturnSupplierInfo(
                supplierReturn.Supplier.ExternalId,
                supplierReturn.Supplier.Name,
                supplierReturn.Supplier.Initial),
            supplierReturn.ReturnDate,
            supplierReturn.ItemCount,
            supplierReturn.Notes,
            supplierReturn.CreatedOn,
            supplierReturn.CreatedBy,
            supplierReturn.Items.Select(i => new ReturnItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.IsRejected)).ToList());
    }
}
