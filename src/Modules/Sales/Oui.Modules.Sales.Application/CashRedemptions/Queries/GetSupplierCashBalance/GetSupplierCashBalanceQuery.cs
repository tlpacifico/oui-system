using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashBalance;

public sealed record GetSupplierCashBalanceQuery(long SupplierId) : IQuery<SupplierCashBalanceResponse>;
