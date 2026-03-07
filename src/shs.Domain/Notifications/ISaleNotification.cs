namespace shs.Domain.Notifications;

public record SaleCompletedNotification(long SaleId, DateTime SaleDate, long[] SoldItemIds);

public interface ISaleNotificationHandler
{
    Task HandleAsync(SaleCompletedNotification notification, CancellationToken ct);
}

public interface ISaleNotificationDispatcher
{
    Task DispatchSaleCompletedAsync(SaleCompletedNotification notification, CancellationToken ct);
}
