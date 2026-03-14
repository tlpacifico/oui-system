using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.UpdateItem;

public sealed record UpdateItemCommand(
    Guid ExternalId,
    string Name,
    string? Description,
    Guid BrandExternalId,
    Guid? CategoryExternalId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    decimal? CommissionPercentage,
    Guid[]? TagExternalIds) : ICommand<ItemDetailResponse>;
