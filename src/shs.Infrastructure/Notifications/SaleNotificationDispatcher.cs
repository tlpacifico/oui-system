using Microsoft.Extensions.Logging;
using shs.Domain.Notifications;

namespace shs.Infrastructure.Notifications;

public class SaleNotificationDispatcher : ISaleNotificationDispatcher
{
    private readonly IEnumerable<ISaleNotificationHandler> _handlers;
    private readonly ILogger<SaleNotificationDispatcher> _logger;

    public SaleNotificationDispatcher(
        IEnumerable<ISaleNotificationHandler> handlers,
        ILogger<SaleNotificationDispatcher> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task DispatchSaleCompletedAsync(SaleCompletedNotification notification, CancellationToken ct)
    {
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.HandleAsync(notification, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in sale notification handler {Handler} for SaleId {SaleId}",
                    handler.GetType().Name, notification.SaleId);
            }
        }
    }
}
