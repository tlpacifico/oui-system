using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Order.NotFound", "Encomenda não encontrada.");

    public static readonly Error OnlyPendingCanBeConfirmed = Error.Problem(
        "Order.OnlyPendingCanBeConfirmed", "Apenas encomendas pendentes podem ser confirmadas.");

    public static readonly Error CannotBeCancelled = Error.Problem(
        "Order.CannotBeCancelled", "Esta encomenda não pode ser cancelada.");
}
