using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.CompleteEvaluation;

public sealed record CompleteEvaluationCommand(Guid ExternalId, string BaseUrl) : ICommand<CompleteEvaluationResponse>;

public sealed record CompleteEvaluationResponse(
    string Message,
    int TotalItems,
    int AcceptedCount,
    int RejectedCount,
    bool EmailSent);
