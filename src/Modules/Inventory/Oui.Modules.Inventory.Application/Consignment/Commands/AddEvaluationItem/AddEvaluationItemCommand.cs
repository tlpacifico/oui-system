using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.AddEvaluationItem;

public sealed record AddEvaluationItemCommand(
    Guid ReceptionExternalId,
    string Name,
    string? Description,
    Guid BrandExternalId,
    Guid? CategoryExternalId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CommissionPercentage,
    bool IsRejected,
    string? RejectionReason,
    Guid[]? TagExternalIds) : ICommand<EvaluationItemResponse>;
