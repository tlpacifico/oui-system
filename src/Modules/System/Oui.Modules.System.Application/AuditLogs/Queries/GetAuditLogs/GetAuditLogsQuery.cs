using shs.Application.Messaging;

namespace Oui.Modules.System.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    string? EntityName,
    string? EntityId,
    string? Action,
    string? UserEmail,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 25) : IQuery<AuditLogPagedResponse>;
