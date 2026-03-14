using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Sales.Queries.GetSaleById;

internal sealed class GetSaleByIdQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSaleByIdQuery, SaleDetailResponse>
{
    public async Task<Result<SaleDetailResponse>> Handle(
        GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await salesDb.Sales
            .Include(s => s.CashRegister)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId, cancellationToken);

        if (sale is null)
            return Result.Failure<SaleDetailResponse>(SaleErrors.NotFound);

        var itemIds = sale.Items.Select(si => si.ItemId).ToList();
        var inventoryItems = await inventoryDb.Items
            .Include(i => i.Brand)
            .Include(i => i.Supplier)
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        return new SaleDetailResponse(
            sale.ExternalId,
            sale.SaleNumber,
            sale.SaleDate,
            sale.Subtotal,
            sale.DiscountPercentage,
            sale.DiscountAmount,
            sale.TotalAmount,
            sale.DiscountReason,
            sale.Status.ToString(),
            sale.Notes,
            sale.CashRegister.OperatorName,
            sale.CashRegister.RegisterNumber,
            sale.Items.Select(si =>
            {
                var item = inventoryItems.GetValueOrDefault(si.ItemId);
                return new SaleItemDetail(
                    item?.ExternalId ?? Guid.Empty,
                    item?.IdentificationNumber ?? "",
                    item?.Name ?? "",
                    item?.Brand?.Name ?? "",
                    item?.Size ?? "",
                    item?.Color ?? "",
                    item?.Supplier?.Name,
                    si.UnitPrice,
                    si.DiscountAmount,
                    si.FinalPrice);
            }).ToList(),
            sale.Payments.Select(p => new SalePaymentDetail(
                p.PaymentMethod.ToString(),
                p.Amount,
                p.Reference)).ToList(),
            sale.CreatedOn);
    }
}
