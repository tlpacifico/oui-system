using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Infrastructure.Services;

public class EcommerceReservationExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EcommerceReservationExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public EcommerceReservationExpirationService(
        IServiceProvider serviceProvider,
        ILogger<EcommerceReservationExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired ecommerce reservations");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShsDbContext>();

        var now = DateTime.UtcNow;

        // Find expired pending orders
        var expiredOrders = await db.EcommerceOrders
            .Include(o => o.Items)
            .Where(o => o.Status == EcommerceOrderStatus.Pending && o.ExpiresAt < now)
            .ToListAsync(ct);

        if (expiredOrders.Count == 0) return;

        _logger.LogInformation("Found {Count} expired ecommerce reservations to cancel", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            order.Status = EcommerceOrderStatus.Cancelled;
            order.CancelledAt = now;
            order.CancellationReason = "Reserva expirada (48h)";

            // Release reserved products back to Published
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var reservedProducts = await db.EcommerceProducts
                .Where(p => productIds.Contains(p.Id) && p.Status == EcommerceProductStatus.Reserved)
                .ToListAsync(ct);

            foreach (var product in reservedProducts)
                product.Status = EcommerceProductStatus.Published;

            _logger.LogInformation("Cancelled expired order {OrderNumber}, released {ProductCount} products",
                order.OrderNumber, reservedProducts.Count);
        }

        await db.SaveChangesAsync(ct);
    }
}
