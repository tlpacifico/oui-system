using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements;

public static class SettlementErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Settlement.NotFound", "Settlement not found.");

    public static readonly Error SupplierNotFound = Error.NotFound(
        "Settlement.SupplierNotFound", "Supplier not found.");

    public static readonly Error NoItemsForSettlement = Error.Problem(
        "Settlement.NoItems", "No items found for settlement in this period.");

    public static readonly Error NoUnsettledItems = Error.Problem(
        "Settlement.NoUnsettledItems", "No unsettled items found for this supplier in the specified period.");

    public static readonly Error AlreadyPaid = Error.Conflict(
        "Settlement.AlreadyPaid", "Settlement is already paid.");

    public static readonly Error IsCancelled = Error.Problem(
        "Settlement.IsCancelled", "Settlement is cancelled.");

    public static readonly Error CannotCancelPaid = Error.Problem(
        "Settlement.CannotCancelPaid", "Cannot cancel a paid settlement.");
}
