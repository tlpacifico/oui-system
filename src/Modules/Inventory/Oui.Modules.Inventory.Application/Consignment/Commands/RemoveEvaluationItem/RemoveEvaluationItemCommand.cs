using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.RemoveEvaluationItem;

public sealed record RemoveEvaluationItemCommand(
    Guid ReceptionExternalId,
    Guid ItemExternalId) : ICommand;
