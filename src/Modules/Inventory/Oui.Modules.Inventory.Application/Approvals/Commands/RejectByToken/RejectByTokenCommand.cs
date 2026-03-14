using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.RejectByToken;

public sealed record RejectByTokenCommand(string Token, string? Message) : ICommand<RejectByTokenResponse>;

public sealed record RejectByTokenResponse(string Message);
