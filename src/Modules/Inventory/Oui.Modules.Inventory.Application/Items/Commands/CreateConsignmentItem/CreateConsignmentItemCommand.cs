using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.CreateConsignmentItem;

public sealed record CreateConsignmentItemCommand(
    Guid ReceptionExternalId,
    string Name,
    string? Description,
    long BrandId,
    long? CategoryId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    long[] TagIds,
    bool IsRejected,
    string? RejectionReason) : ICommand<CreateConsignmentItemResponse>;
