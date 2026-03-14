using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid ExternalId) : ICommand;
