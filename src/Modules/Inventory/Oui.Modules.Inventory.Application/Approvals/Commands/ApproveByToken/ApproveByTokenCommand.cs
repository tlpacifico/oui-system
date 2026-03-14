using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.ApproveByToken;

public sealed record ApproveByTokenCommand(string Token) : ICommand<ApproveByTokenResponse>;

public sealed record ApproveByTokenResponse(string Message, int ItemsApproved);
