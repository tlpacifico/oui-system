using Microsoft.EntityFrameworkCore;
using Oui.Modules.System.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.System.Application.AuditLogs.Queries.GetAuditLogs;

internal sealed class GetAuditLogsQueryHandler(SystemDbContext db)
    : IQueryHandler<GetAuditLogsQuery, AuditLogPagedResponse>
{
    public async Task<Result<AuditLogPagedResponse>> Handle(
        GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom > query.DateTo)
            return Result.Failure<AuditLogPagedResponse>(AuditLogErrors.InvalidDateRange);

        var q = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            q = q.Where(a => a.EntityName == query.EntityName);

        if (!string.IsNullOrWhiteSpace(query.EntityId))
            q = q.Where(a => a.EntityId == query.EntityId);

        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action == query.Action);

        if (!string.IsNullOrWhiteSpace(query.UserEmail))
            q = q.Where(a => a.UserEmail != null && a.UserEmail.Contains(query.UserEmail));

        if (query.DateFrom.HasValue)
            q = q.Where(a => a.Timestamp >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(a => a.Timestamp <= query.DateTo.Value);

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(a => a.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuditLogResponse(
                a.Id,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.OldValues,
                a.NewValues,
                a.ChangedColumns,
                a.UserEmail,
                a.Timestamp))
            .ToListAsync(cancellationToken);

        return new AuditLogPagedResponse(items, totalCount, query.Page, query.PageSize);
    }
}
