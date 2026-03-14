using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid ExternalId) : ICommand;
