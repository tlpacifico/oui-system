using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierById;

public sealed record GetSupplierByIdQuery(Guid ExternalId) : IQuery<SupplierDetailResponse>;
