using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCredits;

public sealed record GetSupplierStoreCreditsQuery(
    long SupplierId,
    StoreCreditStatus? Status) : IQuery<SupplierStoreCreditsResponse>;
