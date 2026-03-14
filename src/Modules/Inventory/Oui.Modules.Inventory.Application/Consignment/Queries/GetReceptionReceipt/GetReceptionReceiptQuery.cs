using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionReceipt;

public sealed record GetReceptionReceiptQuery(Guid ExternalId) : IQuery<string>;
