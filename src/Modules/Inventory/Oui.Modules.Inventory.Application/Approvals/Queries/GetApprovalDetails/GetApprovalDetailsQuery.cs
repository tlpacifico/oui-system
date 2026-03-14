using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Approvals.Queries.GetApprovalDetails;

public sealed record GetApprovalDetailsQuery(string Token) : IQuery<ApprovalDetailsResponse>;
