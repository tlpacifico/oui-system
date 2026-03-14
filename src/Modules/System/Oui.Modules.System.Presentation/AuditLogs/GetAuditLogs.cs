using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.System.Application.AuditLogs.Queries.GetAuditLogs;
using shs.Application.Presentation;

namespace Oui.Modules.System.Presentation.AuditLogs;

internal sealed class GetAuditLogs : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/audit-logs", async (
            string? entityName,
            string? entityId,
            string? action,
            string? userEmail,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAuditLogsQuery(
                entityName,
                entityId,
                action,
                userEmail,
                dateFrom,
                dateTo,
                page ?? 1,
                pageSize ?? 25), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:admin.audit.view")
        .WithTags("Audit Logs");
    }
}
