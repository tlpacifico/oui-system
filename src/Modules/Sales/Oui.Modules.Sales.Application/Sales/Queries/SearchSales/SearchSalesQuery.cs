using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Sales.Queries.SearchSales;

public sealed record SearchSalesQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IQuery<SalesPagedResult>;
