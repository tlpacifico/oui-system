namespace Oui.Modules.System.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record AuditLogResponse(
    long Id,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? ChangedColumns,
    string? UserEmail,
    DateTime Timestamp);

public sealed record AuditLogPagedResponse(
    List<AuditLogResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
