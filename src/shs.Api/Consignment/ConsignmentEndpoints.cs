using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;
using shs.Infrastructure.Services;

namespace shs.Api.Consignment;

public static class ConsignmentEndpoints
{
    public static void MapConsignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consignments")
            .WithTags("Consignments")
            .RequireAuthorization();

        // Reception endpoints
        group.MapPost("/receptions", CreateReception);
        group.MapGet("/receptions", GetReceptions);
        group.MapGet("/receptions/{externalId:guid}", GetReceptionById);
        group.MapGet("/receptions/{externalId:guid}/receipt", GetReceptionReceipt);

        // Evaluation endpoints (CU-09)
        group.MapPost("/receptions/{externalId:guid}/items", AddEvaluationItem);
        group.MapGet("/receptions/{externalId:guid}/items", GetReceptionItems);
        group.MapDelete("/receptions/{receptionExternalId:guid}/items/{itemExternalId:guid}", RemoveEvaluationItem);
        group.MapPut("/receptions/{externalId:guid}/complete-evaluation", CompleteEvaluation);

        // Email endpoint (CU-10)
        group.MapPost("/receptions/{externalId:guid}/send-evaluation-email", SendEvaluationEmail);
    }

    /// <summary>
    /// CU-08: Create a new reception — records supplier, item count, and notes.
    /// Status starts as PendingEvaluation.
    /// </summary>
    private static async Task<IResult> CreateReception(
        [FromBody] CreateReceptionRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        // Validate supplier
        if (!req.SupplierExternalId.HasValue)
            return Results.BadRequest(new { error = "O fornecedor é obrigatório." });

        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == req.SupplierExternalId.Value, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        // Validate item count
        if (req.ItemCount <= 0)
            return Results.BadRequest(new { error = "A quantidade de itens deve ser maior que zero." });

        if (req.ItemCount > 500)
            return Results.BadRequest(new { error = "A quantidade máxima por recepção é de 500 itens." });

        var reception = new ReceptionEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = supplier.Id,
            ReceptionDate = DateTime.UtcNow,
            ItemCount = req.ItemCount,
            Status = ReceptionStatus.PendingEvaluation,
            Notes = req.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        db.Receptions.Add(reception);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/consignments/receptions/{reception.ExternalId}",
            new ReceptionDetailResponse(
                reception.ExternalId,
                new ReceptionSupplierInfo(supplier.ExternalId, supplier.Name, supplier.Initial),
                reception.ReceptionDate,
                reception.ItemCount,
                reception.Status.ToString(),
                reception.Notes,
                0, // evaluatedCount
                0, // acceptedCount
                0, // rejectedCount
                reception.EvaluatedAt,
                reception.EvaluatedBy,
                reception.CreatedOn,
                reception.CreatedBy
            )
        );
    }

    /// <summary>
    /// List all receptions with optional filtering by status or supplier.
    /// </summary>
    private static async Task<IResult> GetReceptions(
        [FromServices] ShsDbContext db,
        [FromQuery] string? status,
        [FromQuery] Guid? supplierExternalId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Receptions
            .Include(r => r.Supplier)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReceptionStatus>(status, out var receptionStatus))
            query = query.Where(r => r.Status == receptionStatus);

        // Filter by supplier
        if (supplierExternalId.HasValue)
        {
            var supplier = await db.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ExternalId == supplierExternalId.Value, ct);

            if (supplier is not null)
                query = query.Where(r => r.SupplierId == supplier.Id);
        }

        // Search by supplier name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Supplier.Name.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(ct);

        var receptions = await query
            .OrderByDescending(r => r.ReceptionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReceptionListItemResponse(
                r.ExternalId,
                new ReceptionSupplierInfo(r.Supplier.ExternalId, r.Supplier.Name, r.Supplier.Initial),
                r.ReceptionDate,
                r.ItemCount,
                r.Status.ToString(),
                r.Items.Count(i => !i.IsDeleted),
                r.Items.Count(i => !i.IsDeleted && !i.IsRejected && i.Status != ItemStatus.Rejected),
                r.Items.Count(i => !i.IsDeleted && i.IsRejected),
                r.Notes,
                r.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(new ReceptionPagedResult(
            receptions,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        ));
    }

    /// <summary>
    /// Get a single reception with full details.
    /// </summary>
    private static async Task<IResult> GetReceptionById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        return Results.Ok(new ReceptionDetailResponse(
            reception.ExternalId,
            new ReceptionSupplierInfo(reception.Supplier.ExternalId, reception.Supplier.Name, reception.Supplier.Initial),
            reception.ReceptionDate,
            reception.ItemCount,
            reception.Status.ToString(),
            reception.Notes,
            reception.Items.Count,
            reception.Items.Count(i => !i.IsRejected && i.Status != ItemStatus.Rejected),
            reception.Items.Count(i => i.IsRejected),
            reception.EvaluatedAt,
            reception.EvaluatedBy,
            reception.CreatedOn,
            reception.CreatedBy
        ));
    }

    /// <summary>
    /// Generate a printable receipt for the reception.
    /// Returns HTML that can be printed from the browser.
    /// Receipt shows: date, supplier name, item count, and a signature line.
    /// No prices or values are shown (per business rules).
    /// </summary>
    private static async Task<IResult> GetReceptionReceipt(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        var html = GenerateReceiptHtml(reception);

        return Results.Content(html, "text/html; charset=utf-8");
    }

    // ── Evaluation Endpoints (CU-09) ──

    /// <summary>
    /// Add an evaluated item to a reception. Each item gets its full details
    /// (name, brand, condition, price, etc.) and can be accepted or rejected.
    /// </summary>
    private static async Task<IResult> AddEvaluationItem(
        Guid externalId,
        [FromBody] AddEvaluationItemRequest req,
        [FromServices] ShsDbContext db,
        [FromServices] IItemIdGeneratorService idGenerator,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Results.Conflict(new { error = "Esta recepção já foi avaliada." });

        // Check if we already have enough items
        var currentItemCount = reception.Items.Count;
        if (currentItemCount >= reception.ItemCount)
            return Results.Conflict(new { error = $"Já foram avaliados {currentItemCount} de {reception.ItemCount} itens. Não é possível adicionar mais." });

        // Validate required fields
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "O nome é obrigatório." });
        if (req.EvaluatedPrice <= 0)
            return Results.BadRequest(new { error = "O preço deve ser maior que zero." });
        if (string.IsNullOrWhiteSpace(req.Size))
            return Results.BadRequest(new { error = "O tamanho é obrigatório." });
        if (string.IsNullOrWhiteSpace(req.Color))
            return Results.BadRequest(new { error = "A cor é obrigatória." });

        // Validate brand
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == req.BrandExternalId, ct);
        if (brand is null)
            return Results.BadRequest(new { error = "Marca não encontrada." });

        // Validate category (optional)
        long? categoryId = null;
        if (req.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == req.CategoryExternalId.Value, ct);
            if (category is null)
                return Results.BadRequest(new { error = "Categoria não encontrada." });
            categoryId = category.Id;
        }

        // Validate condition
        if (!Enum.TryParse<ItemCondition>(req.Condition, out var condition))
            return Results.BadRequest(new { error = "Condição inválida." });

        // Generate ID
        var itemId = await idGenerator.GenerateNextIdAsync(reception.SupplierId, ct);

        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            BrandId = brand.Id,
            CategoryId = categoryId,
            Size = req.Size.Trim(),
            Color = req.Color.Trim(),
            Composition = req.Composition?.Trim(),
            Condition = condition,
            EvaluatedPrice = req.EvaluatedPrice,
            Status = req.IsRejected ? ItemStatus.Rejected : ItemStatus.Evaluated,
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = reception.SupplierId,
            ReceptionId = reception.Id,
            CommissionPercentage = req.CommissionPercentage ?? 50m,
            IsRejected = req.IsRejected,
            RejectionReason = req.IsRejected ? req.RejectionReason?.Trim() : null,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        // Add tags
        if (req.TagExternalIds is { Length: > 0 })
        {
            var tags = await db.Tags
                .Where(t => req.TagExternalIds.Contains(t.ExternalId))
                .ToListAsync(ct);
            item.Tags = tags;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(ct);

        // Reload brand for response
        await db.Entry(item).Reference(i => i.Brand).LoadAsync(ct);

        return Results.Created(
            $"/api/inventory/items/{item.ExternalId}",
            new EvaluationItemResponse(
                item.ExternalId,
                item.IdentificationNumber,
                item.Name,
                item.Brand.Name,
                item.Size,
                item.Color,
                item.Condition.ToString(),
                item.EvaluatedPrice,
                item.CommissionPercentage,
                item.Status.ToString(),
                item.IsRejected,
                item.RejectionReason,
                item.CreatedOn
            )
        );
    }

    /// <summary>
    /// List all items in a reception (evaluated so far).
    /// </summary>
    private static async Task<IResult> GetReceptionItems(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        var items = await db.Items
            .Where(i => i.ReceptionId == reception.Id && !i.IsDeleted)
            .Include(i => i.Brand)
            .OrderBy(i => i.CreatedOn)
            .Select(i => new EvaluationItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.CommissionPercentage,
                i.Status.ToString(),
                i.IsRejected,
                i.RejectionReason,
                i.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(items);
    }

    /// <summary>
    /// Remove an item from the evaluation (undo).
    /// Only allowed while the reception is still PendingEvaluation.
    /// </summary>
    private static async Task<IResult> RemoveEvaluationItem(
        Guid receptionExternalId,
        Guid itemExternalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == receptionExternalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Results.Conflict(new { error = "Não é possível remover itens de uma recepção já avaliada." });

        var item = await db.Items
            .FirstOrDefaultAsync(i => i.ExternalId == itemExternalId && i.ReceptionId == reception.Id, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado nesta recepção." });

        // Soft delete the item
        item.IsDeleted = true;
        item.DeletedBy = "system"; // TODO: Get from JWT claims
        item.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    /// <summary>
    /// Complete the evaluation of a reception.
    /// Validates that the number of evaluated items matches the expected count.
    /// Changes the reception status to Evaluated.
    /// Automatically sends the evaluation email to the supplier.
    /// </summary>
    private static async Task<IResult> CompleteEvaluation(
        Guid externalId,
        [FromServices] ShsDbContext db,
        [FromServices] IEmailService emailService,
        [FromServices] ILogger<EmailService> logger,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Results.Conflict(new { error = "Esta recepção já foi avaliada." });

        var evaluatedCount = reception.Items.Count;
        if (evaluatedCount == 0)
            return Results.BadRequest(new { error = "Não é possível concluir a avaliação sem nenhum item avaliado." });

        if (evaluatedCount < reception.ItemCount)
            return Results.BadRequest(new { error = $"Faltam avaliar {reception.ItemCount - evaluatedCount} de {reception.ItemCount} itens." });

        // Mark accepted items as ToSell
        foreach (var item in reception.Items.Where(i => !i.IsRejected))
        {
            item.Status = ItemStatus.ToSell;
        }

        reception.Status = ReceptionStatus.Evaluated;
        reception.EvaluatedAt = DateTime.UtcNow;
        reception.EvaluatedBy = "system"; // TODO: Get from JWT claims
        reception.UpdatedOn = DateTime.UtcNow;
        reception.UpdatedBy = "system";

        await db.SaveChangesAsync(ct);

        // Auto-send evaluation email (best-effort, don't fail the request)
        var emailSent = false;
        try
        {
            var emailData = BuildEvaluationEmailData(reception);
            await emailService.SendEvaluationEmailAsync(emailData, ct);
            emailSent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to auto-send evaluation email for reception {Id}", reception.ExternalId);
        }

        return Results.Ok(new
        {
            message = "Avaliação concluída com sucesso.",
            totalItems = reception.ItemCount,
            acceptedCount = reception.Items.Count(i => !i.IsRejected),
            rejectedCount = reception.Items.Count(i => i.IsRejected),
            emailSent
        });
    }

    /// <summary>
    /// CU-10: Manually send (or re-send) the evaluation email to the supplier.
    /// Only allowed after the reception has been evaluated.
    /// </summary>
    private static async Task<IResult> SendEvaluationEmail(
        Guid externalId,
        [FromServices] ShsDbContext db,
        [FromServices] IEmailService emailService,
        CancellationToken ct)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (reception is null)
            return Results.NotFound(new { error = "Recepção não encontrada." });

        if (reception.Status == ReceptionStatus.PendingEvaluation)
            return Results.BadRequest(new { error = "A avaliação ainda não foi concluída." });

        try
        {
            var emailData = BuildEvaluationEmailData(reception);
            await emailService.SendEvaluationEmailAsync(emailData, ct);

            return Results.Ok(new
            {
                message = "Email de avaliação enviado com sucesso.",
                sentTo = reception.Supplier.Email
            });
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = "Erro ao enviar email: " + ex.Message }, statusCode: 500);
        }
    }

    private static EvaluationEmailData BuildEvaluationEmailData(ReceptionEntity reception)
    {
        return new EvaluationEmailData
        {
            SupplierName = reception.Supplier.Name,
            SupplierEmail = reception.Supplier.Email,
            ReceptionDate = reception.ReceptionDate,
            ReceptionRef = reception.ExternalId.ToString()[..8].ToUpper(),
            AcceptedItems = reception.Items
                .Where(i => !i.IsRejected)
                .Select(i => new EvaluationEmailItem
                {
                    IdentificationNumber = i.IdentificationNumber,
                    Name = i.Name,
                    Brand = i.Brand.Name,
                    Size = i.Size,
                    Color = i.Color,
                    Condition = i.Condition.ToString(),
                    EvaluatedPrice = i.EvaluatedPrice,
                })
                .ToList(),
            RejectedItems = reception.Items
                .Where(i => i.IsRejected)
                .Select(i => new EvaluationEmailItem
                {
                    IdentificationNumber = i.IdentificationNumber,
                    Name = i.Name,
                    Brand = i.Brand.Name,
                    Size = i.Size,
                    Color = i.Color,
                    Condition = i.Condition.ToString(),
                    EvaluatedPrice = i.EvaluatedPrice,
                    RejectionReason = i.RejectionReason,
                })
                .ToList(),
        };
    }

    private static string GenerateReceiptHtml(ReceptionEntity reception)
    {
        var receptionDate = reception.ReceptionDate.ToString("dd/MM/yyyy HH:mm");
        var supplierName = System.Net.WebUtility.HtmlEncode(reception.Supplier.Name);
        var supplierNif = System.Net.WebUtility.HtmlEncode(reception.Supplier.TaxNumber ?? "\u2014");
        var itemCount = reception.ItemCount;
        var notes = System.Net.WebUtility.HtmlEncode(reception.Notes ?? "");
        var receiptId = reception.ExternalId.ToString()[..8].ToUpper();
        var itemLabel = itemCount == 1 ? "pe\u00e7a recebida" : "pe\u00e7as recebidas";
        var generatedAt = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");

        var notesHtml = string.IsNullOrWhiteSpace(reception.Notes)
            ? ""
            : "<div class=\"notes-section\">"
              + "<h3>Observa\u00e7\u00f5es:</h3>"
              + "<p>" + notes + "</p>"
              + "</div>";

        return "<!DOCTYPE html>"
            + "<html lang=\"pt\"><head><meta charset=\"UTF-8\">"
            + "<title>Recibo de Recep\u00e7\u00e3o - " + receiptId + "</title>"
            + "<style>"
            + "* { margin: 0; padding: 0; box-sizing: border-box; }"
            + "body { font-family: 'Segoe UI', Arial, sans-serif; padding: 40px; max-width: 600px; margin: 0 auto; color: #1e293b; line-height: 1.6; }"
            + ".header { text-align: center; margin-bottom: 32px; padding-bottom: 20px; border-bottom: 2px solid #1e293b; }"
            + ".header h1 { font-size: 22px; font-weight: 700; margin-bottom: 4px; }"
            + ".header .subtitle { font-size: 13px; color: #64748b; }"
            + ".receipt-title { text-align: center; font-size: 18px; font-weight: 700; margin-bottom: 24px; text-transform: uppercase; letter-spacing: 1px; }"
            + ".receipt-id { text-align: center; font-size: 13px; color: #64748b; margin-bottom: 24px; }"
            + ".info-table { width: 100%; margin-bottom: 24px; }"
            + ".info-table td { padding: 8px 0; vertical-align: top; }"
            + ".info-table .label { font-weight: 600; width: 180px; color: #374151; }"
            + ".info-table .value { color: #1e293b; }"
            + ".items-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; text-align: center; margin-bottom: 24px; }"
            + ".items-count { font-size: 48px; font-weight: 700; color: #6366f1; }"
            + ".items-label { font-size: 14px; color: #64748b; margin-top: 4px; }"
            + ".notes-section { margin-bottom: 32px; }"
            + ".notes-section h3 { font-size: 14px; font-weight: 600; margin-bottom: 8px; }"
            + ".notes-section p { font-size: 13px; color: #475569; padding: 12px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; min-height: 40px; }"
            + ".signature-section { margin-top: 48px; display: flex; justify-content: space-between; gap: 40px; }"
            + ".signature-block { flex: 1; text-align: center; }"
            + ".signature-line { border-top: 1px solid #1e293b; margin-top: 60px; padding-top: 8px; font-size: 13px; color: #64748b; }"
            + ".footer { margin-top: 40px; text-align: center; font-size: 11px; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 16px; }"
            + ".disclaimer { margin-top: 24px; font-size: 12px; color: #64748b; text-align: center; font-style: italic; }"
            + "@media print { body { padding: 20px; } .no-print { display: none; } }"
            + ".print-btn { display: block; margin: 0 auto 32px; padding: 10px 32px; background: #6366f1; color: white; border: none; border-radius: 8px; font-size: 14px; font-weight: 600; cursor: pointer; }"
            + ".print-btn:hover { background: #4f46e5; }"
            + "</style></head><body>"
            + "<button class=\"print-btn no-print\" onclick=\"this.style.display='none'; window.print();\">Imprimir Recibo</button>"
            + "<div class=\"header\"><h1>OUI System</h1><span class=\"subtitle\">Second Hand Shop ERP</span></div>"
            + "<div class=\"receipt-title\">Recibo de Recep\u00e7\u00e3o</div>"
            + "<div class=\"receipt-id\">Ref: " + receiptId + "</div>"
            + "<table class=\"info-table\">"
            + "<tr><td class=\"label\">Data de Recep\u00e7\u00e3o:</td><td class=\"value\">" + receptionDate + "</td></tr>"
            + "<tr><td class=\"label\">Fornecedor:</td><td class=\"value\">" + supplierName + "</td></tr>"
            + "<tr><td class=\"label\">NIF do Fornecedor:</td><td class=\"value\">" + supplierNif + "</td></tr>"
            + "</table>"
            + "<div class=\"items-box\">"
            + "<div class=\"items-count\">" + itemCount + "</div>"
            + "<div class=\"items-label\">" + itemLabel + "</div>"
            + "</div>"
            + notesHtml
            + "<p class=\"disclaimer\">"
            + "Este recibo confirma apenas a recep\u00e7\u00e3o f\u00edsica dos itens indicados. "
            + "A avalia\u00e7\u00e3o, precifica\u00e7\u00e3o e aceita\u00e7\u00e3o das pe\u00e7as ser\u00e1 comunicada posteriormente."
            + "</p>"
            + "<div class=\"signature-section\">"
            + "<div class=\"signature-block\"><div class=\"signature-line\">Fornecedor</div></div>"
            + "<div class=\"signature-block\"><div class=\"signature-line\">Loja (OUI)</div></div>"
            + "</div>"
            + "<div class=\"footer\">"
            + "Documento gerado automaticamente pelo OUI System em " + generatedAt + " UTC"
            + "</div>"
            + "</body></html>";
    }
}

// ── Request DTOs ──

public record CreateReceptionRequest(
    Guid? SupplierExternalId,
    int ItemCount,
    string? Notes
);

// ── Response DTOs ──

public record ReceptionSupplierInfo(
    Guid ExternalId,
    string Name,
    string Initial
);

public record ReceptionListItemResponse(
    Guid ExternalId,
    ReceptionSupplierInfo Supplier,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    string? Notes,
    DateTime CreatedOn
);

public record ReceptionDetailResponse(
    Guid ExternalId,
    ReceptionSupplierInfo Supplier,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    string? Notes,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    DateTime? EvaluatedAt,
    string? EvaluatedBy,
    DateTime CreatedOn,
    string? CreatedBy
);

public record ReceptionPagedResult(
    List<ReceptionListItemResponse> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ── Evaluation DTOs ──

public record AddEvaluationItemRequest(
    string Name,
    string? Description,
    Guid BrandExternalId,
    Guid? CategoryExternalId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CommissionPercentage,
    bool IsRejected,
    string? RejectionReason,
    Guid[]? TagExternalIds
);

public record EvaluationItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    decimal CommissionPercentage,
    string Status,
    bool IsRejected,
    string? RejectionReason,
    DateTime CreatedOn
);
