using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand(Guid ExternalId) : ICommand;
