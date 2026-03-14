using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Commands.CreateReturn;

internal sealed class CreateReturnCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateReturnCommand, SupplierReturnDetailResponse>
{
    public async Task<Result<SupplierReturnDetailResponse>> Handle(
        CreateReturnCommand request, CancellationToken cancellationToken)
    {
        if (request.ItemExternalIds is null || request.ItemExternalIds.Length == 0)
            return Result.Failure<SupplierReturnDetailResponse>(SupplierReturnErrors.NoItemsSelected);

        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == request.SupplierExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure<SupplierReturnDetailResponse>(SupplierReturnErrors.SupplierNotFound);

        var items = await db.Items
            .Where(i => request.ItemExternalIds.Contains(i.ExternalId)
                        && i.SupplierId == supplier.Id
                        && !i.IsDeleted)
            .Include(i => i.Brand)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return Result.Failure<SupplierReturnDetailResponse>(SupplierReturnErrors.NoValidItems);

        var nonReturnable = items
            .Where(i => i.Status != ItemStatus.ToSell && i.Status != ItemStatus.AwaitingAcceptance && i.Status != ItemStatus.Rejected)
            .ToList();

        if (nonReturnable.Count > 0)
        {
            var ids = string.Join(", ", nonReturnable.Select(i => i.IdentificationNumber));
            return Result.Failure<SupplierReturnDetailResponse>(SupplierReturnErrors.ItemsNotReturnable(ids));
        }

        var supplierReturn = new SupplierReturnEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = supplier.Id,
            ReturnDate = DateTime.UtcNow,
            ItemCount = items.Count,
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.SupplierReturns.Add(supplierReturn);

        foreach (var item in items)
        {
            item.Status = ItemStatus.Returned;
            item.SupplierReturnId = supplierReturn.Id;
            item.ReturnedAt = DateTime.UtcNow;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "system";
        }

        supplierReturn.Items = items;

        await db.SaveChangesAsync(cancellationToken);

        return new SupplierReturnDetailResponse(
            supplierReturn.ExternalId,
            new ReturnSupplierInfo(supplier.ExternalId, supplier.Name, supplier.Initial),
            supplierReturn.ReturnDate,
            supplierReturn.ItemCount,
            supplierReturn.Notes,
            supplierReturn.CreatedOn,
            supplierReturn.CreatedBy,
            items.Select(i => new ReturnItemResponse(
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
