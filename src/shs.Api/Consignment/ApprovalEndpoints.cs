using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Enums;
using Oui.Modules.Inventory.Infrastructure;

namespace shs.Api.Consignment;

public static class ApprovalEndpoints
{
    public static void MapApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        // Public endpoints (no auth) — supplier uses token link
        var publicGroup = app.MapGroup("/api/consignments/approval").WithTags("ConsignmentApproval");
        publicGroup.MapGet("/{token}", GetApprovalDetails).AllowAnonymous();
        publicGroup.MapPost("/{token}/approve", ApproveByToken).AllowAnonymous();
        publicGroup.MapPost("/{token}/reject", RejectByToken).AllowAnonymous();

        // Staff endpoint (auth required)
        var staffGroup = app.MapGroup("/api/consignments/receptions").WithTags("Consignments");
        staffGroup.MapPut("/{externalId:guid}/approve", StaffApprove).RequirePermission("consignment.receptions.approve");
    }

    /// <summary>
    /// Public: Get reception details for supplier approval page.
    /// </summary>
    private static async Task<IResult> GetApprovalDetails(
        string token,
        [FromServices] InventoryDbContext db,
        CancellationToken ct)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
                .ThenInclude(r => r.Supplier)
            .Include(t => t.Reception)
                .ThenInclude(r => r.Items.Where(i => !i.IsDeleted && !i.IsRejected))
                    .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (approvalToken is null)
            return Results.NotFound(new { error = "Link de aprovação inválido." });

        if (approvalToken.IsUsed)
            return Results.Conflict(new { error = "Esta aprovação já foi processada.", approvedAt = approvalToken.ApprovedAt });

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Results.BadRequest(new { error = "Este link de aprovação expirou. Contacte a loja para obter um novo link." });

        var reception = approvalToken.Reception;
        var items = reception.Items
            .Where(i => i.Status == ItemStatus.AwaitingAcceptance)
            .Select(i => new ApprovalItemResponse(
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.CommissionPercentage
            ))
            .ToList();

        return Results.Ok(new ApprovalDetailsResponse(
            reception.Supplier.Name,
            reception.ReceptionDate,
            reception.ExternalId.ToString()[..8].ToUpper(),
            items,
            items.Sum(i => i.EvaluatedPrice),
            approvalToken.ExpiresAt
        ));
    }

    /// <summary>
    /// Public: Supplier approves via token link.
    /// </summary>
    private static async Task<IResult> ApproveByToken(
        string token,
        [FromServices] InventoryDbContext db,
        CancellationToken ct)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
                .ThenInclude(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (approvalToken is null)
            return Results.NotFound(new { error = "Link de aprovação inválido." });

        if (approvalToken.IsUsed)
            return Results.Conflict(new { error = "Esta aprovação já foi processada." });

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Results.BadRequest(new { error = "Este link de aprovação expirou." });

        // Move items from AwaitingAcceptance to ToSell
        var itemsApproved = 0;
        foreach (var item in approvalToken.Reception.Items.Where(i => i.Status == ItemStatus.AwaitingAcceptance))
        {
            item.Status = ItemStatus.ToSell;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "supplier";
            itemsApproved++;
        }

        approvalToken.IsUsed = true;
        approvalToken.ApprovedAt = DateTime.UtcNow;
        approvalToken.ApprovedBy = "supplier (via link)";
        approvalToken.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            message = "Aprovação registada com sucesso! As peças serão colocadas à venda.",
            itemsApproved
        });
    }

    /// <summary>
    /// Public: Supplier rejects via token link (with optional message).
    /// </summary>
    private static async Task<IResult> RejectByToken(
        string token,
        [FromBody] ApprovalRejectRequest req,
        [FromServices] InventoryDbContext db,
        CancellationToken ct)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (approvalToken is null)
            return Results.NotFound(new { error = "Link de aprovação inválido." });

        if (approvalToken.IsUsed)
            return Results.Conflict(new { error = "Esta aprovação já foi processada." });

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Results.BadRequest(new { error = "Este link de aprovação expirou." });

        approvalToken.IsUsed = true;
        approvalToken.ApprovedAt = DateTime.UtcNow;
        approvalToken.ApprovedBy = $"supplier (rejected: {req.Message?.Trim() ?? "sem motivo"})";
        approvalToken.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            message = "A sua resposta foi registada. A equipa da loja entrará em contacto consigo."
        });
    }

    /// <summary>
    /// Staff: Approve a reception manually (e.g., supplier confirmed via WhatsApp/phone).
    /// </summary>
    private static async Task<IResult> StaffApprove(
        Guid externalId,
        [FromServices] InventoryDbContext db,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        if (reception.Status != ReceptionStatus.Evaluated)
            return Results.Conflict(new { error = "Esta recepção não está no estado de avaliação concluída." });

        var awaitingItems = reception.Items.Where(i => i.Status == ItemStatus.AwaitingAcceptance).ToList();
        if (awaitingItems.Count == 0)
            return Results.Conflict(new { error = "Não existem itens a aguardar aprovação nesta recepção." });

        foreach (var item in awaitingItems)
        {
            item.Status = ItemStatus.ToSell;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "system"; // TODO: Get from JWT claims
        }

        // Mark any unused tokens as used
        var unusedTokens = await db.ReceptionApprovalTokens
            .Where(t => t.ReceptionId == reception.Id && !t.IsUsed)
            .ToListAsync(ct);

        foreach (var token in unusedTokens)
        {
            token.IsUsed = true;
            token.ApprovedAt = DateTime.UtcNow;
            token.ApprovedBy = "staff"; // TODO: Get from JWT claims
            token.UpdatedOn = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            message = "Aprovação registada com sucesso.",
            itemsApproved = awaitingItems.Count
        });
    }
}

// ── DTOs ──

public record ApprovalDetailsResponse(
    string SupplierName,
    DateTime ReceptionDate,
    string ReceptionRef,
    List<ApprovalItemResponse> Items,
    decimal TotalValue,
    DateTime ExpiresAt
);

public record ApprovalItemResponse(
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    decimal CommissionPercentage
);

public record ApprovalRejectRequest(
    string? Message
);
