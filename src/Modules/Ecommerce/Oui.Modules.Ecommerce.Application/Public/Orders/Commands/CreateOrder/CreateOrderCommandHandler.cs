using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandHandler(EcommerceDbContext db)
    : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Result.Failure<CreateOrderResponse>(StoreOrderErrors.CustomerNameRequired);

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            return Result.Failure<CreateOrderResponse>(StoreOrderErrors.CustomerEmailRequired);

        if (request.ProductSlugs is null || request.ProductSlugs.Count == 0)
            return Result.Failure<CreateOrderResponse>(StoreOrderErrors.NoProductsSelected);

        var products = await db.EcommerceProducts
            .Where(p => request.ProductSlugs.Contains(p.Slug) && p.Status == EcommerceProductStatus.Published)
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
            return Result.Failure<CreateOrderResponse>(StoreOrderErrors.NoProductsAvailable);

        var unavailableSlugs = request.ProductSlugs.Except(products.Select(p => p.Slug)).ToList();

        // Generate order number: EC{YYYYMMDD}-{DailySequence:000}
        var today = DateTime.UtcNow;
        var datePrefix = $"EC{today:yyyyMMdd}";
        var lastOrderToday = await db.EcommerceOrders
            .Where(o => o.OrderNumber.StartsWith(datePrefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastOrderToday is not null)
        {
            var lastSeq = lastOrderToday.Split('-').LastOrDefault();
            if (int.TryParse(lastSeq, out var parsed))
                sequence = parsed + 1;
        }

        var orderNumber = $"{datePrefix}-{sequence:D3}";
        var totalAmount = products.Sum(p => p.Price);

        var order = new EcommerceOrderEntity
        {
            ExternalId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            Status = EcommerceOrderStatus.Pending,
            TotalAmount = totalAmount,
            Notes = request.Notes?.Trim(),
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "customer"
        };

        foreach (var product in products)
        {
            order.Items.Add(new EcommerceOrderItemEntity
            {
                ExternalId = Guid.NewGuid(),
                ProductId = product.Id,
                ItemId = product.ItemId,
                ProductTitle = product.Title,
                Price = product.Price,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "customer"
            });

            product.Status = EcommerceProductStatus.Reserved;
        }

        db.EcommerceOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(
            order.ExternalId,
            order.OrderNumber,
            order.CustomerName,
            order.TotalAmount,
            order.ReservedAt,
            order.ExpiresAt,
            order.Items.Select(i => new CreateOrderItemResponse(i.ProductTitle, i.Price)).ToList(),
            unavailableSlugs);
    }
}
