using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.StaffApprove;

public sealed record StaffApproveCommand(Guid ExternalId) : ICommand<StaffApproveResponse>;

public sealed record StaffApproveResponse(string Message, int ItemsApproved);
