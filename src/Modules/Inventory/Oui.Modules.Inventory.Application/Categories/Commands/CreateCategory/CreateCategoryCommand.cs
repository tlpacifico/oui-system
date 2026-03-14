using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    Guid? ParentCategoryExternalId) : ICommand<CategoryDetailResponse>;
