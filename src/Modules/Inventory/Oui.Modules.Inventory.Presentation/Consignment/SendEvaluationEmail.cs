using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Commands.SendEvaluationEmail;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class SendEvaluationEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/receptions/{externalId:guid}/send-evaluation-email", async (
            Guid externalId,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var result = await sender.Send(new SendEvaluationEmailCommand(externalId, baseUrl), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.evaluate")
        .WithTags("Consignment");
    }
}
