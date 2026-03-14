using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCreditBalance;

public sealed record GetSupplierStoreCreditBalanceQuery(long SupplierId) : IQuery<SupplierStoreCreditBalanceResponse>;
