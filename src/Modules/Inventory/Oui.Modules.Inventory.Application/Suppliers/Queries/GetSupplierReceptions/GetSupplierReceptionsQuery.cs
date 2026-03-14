using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierReceptions;

public sealed record GetSupplierReceptionsQuery(Guid ExternalId) : IQuery<List<SupplierReceptionResponse>>;
