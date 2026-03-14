using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetStoreCreditTransactions;

public sealed record GetStoreCreditTransactionsQuery(Guid ExternalId) : IQuery<StoreCreditTransactionsResponse>;
