using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid ExternalId,
    string Name,
    string? Description,
    Guid? ParentCategoryExternalId) : ICommand<CategoryDetailResponse>;
