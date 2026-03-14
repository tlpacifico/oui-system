using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashRedemptionHistory;

public sealed record GetSupplierCashRedemptionHistoryQuery(
    long SupplierId,
    int Page = 1,
    int PageSize = 20) : IQuery<SupplierCashRedemptionHistoryResponse>;
