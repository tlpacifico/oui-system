using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.SendEvaluationEmail;

public sealed record SendEvaluationEmailCommand(Guid ExternalId, string BaseUrl) : ICommand<SendEvaluationEmailResponse>;

public sealed record SendEvaluationEmailResponse(string Message, string SentTo);
