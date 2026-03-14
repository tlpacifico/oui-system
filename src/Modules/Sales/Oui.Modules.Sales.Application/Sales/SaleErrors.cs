using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Sales;

public static class SaleErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Sale.NotFound", "Venda não encontrada.");
}
