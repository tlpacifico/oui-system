using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.CreateItem;

public sealed record CreateItemCommand(
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
    string AcquisitionType,
    string Origin,
    Guid? SupplierExternalId,
    decimal? CommissionPercentage,
    Guid[]? TagExternalIds) : ICommand<CreateConsignmentItemResponse>;
