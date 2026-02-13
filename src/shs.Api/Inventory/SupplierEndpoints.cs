using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Inventory;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers").WithTags("Suppliers");

        group.MapGet("/", GetAll).RequirePermission("inventory.suppliers.manage");
        group.MapGet("/{externalId:guid}", GetById).RequirePermission("inventory.suppliers.manage");
        group.MapGet("/{externalId:guid}/items", GetSupplierItems).RequirePermission("inventory.suppliers.manage");
        group.MapGet("/{externalId:guid}/receptions", GetSupplierReceptions).RequirePermission("inventory.suppliers.manage");
        group.MapPost("/", Create).RequirePermission("inventory.suppliers.manage");
        group.MapPut("/{externalId:guid}", Update).RequirePermission("inventory.suppliers.manage");
        group.MapDelete("/{externalId:guid}", Delete).RequirePermission("inventory.suppliers.manage");
    }

    private static async Task<IResult> GetAll(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(sup =>
                sup.Name.ToLower().Contains(s) ||
                sup.Email.ToLower().Contains(s) ||
                sup.PhoneNumber.Contains(s) ||
                (sup.TaxNumber != null && sup.TaxNumber.Contains(s)) ||
                sup.Initial.ToLower().Contains(s));
        }

        var suppliers = await query
            .OrderBy(sup => sup.Name)
            .Select(sup => new SupplierListResponse(
                sup.ExternalId,
                sup.Name,
                sup.Email,
                sup.PhoneNumber,
                sup.TaxNumber,
                sup.Initial,
                sup.Items.Count(i => !i.IsDeleted),
                sup.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(suppliers);
    }

    private static async Task<IResult> GetById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .Where(sup => sup.ExternalId == externalId)
            .Select(sup => new SupplierDetailResponse(
                sup.ExternalId,
                sup.Name,
                sup.Email,
                sup.PhoneNumber,
                sup.TaxNumber,
                sup.Initial,
                sup.Notes,
                sup.Items.Count(i => !i.IsDeleted),
                sup.CreatedOn,
                sup.CreatedBy,
                sup.UpdatedOn,
                sup.UpdatedBy
            ))
            .FirstOrDefaultAsync(ct);

        return supplier is null
            ? Results.NotFound(new { error = "Fornecedor não encontrado." })
            : Results.Ok(supplier);
    }

    private static async Task<IResult> GetSupplierItems(
        Guid externalId,
        [FromServices] ShsDbContext db,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        var query = db.Items
            .Where(i => i.SupplierId == supplier.Id)
            .Include(i => i.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ItemStatus>(status, out var itemStatus))
            query = query.Where(i => i.Status == itemStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new SupplierItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.Condition.ToString(),
                i.DaysInStock,
                i.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(new
        {
            data = items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    private static async Task<IResult> GetSupplierReceptions(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        var receptions = await db.Receptions
            .Where(r => r.SupplierId == supplier.Id)
            .OrderByDescending(r => r.ReceptionDate)
            .Select(r => new SupplierReceptionResponse(
                r.ExternalId,
                r.ReceptionDate,
                r.ItemCount,
                r.Status.ToString(),
                r.Items.Count(i => !i.IsDeleted),
                r.Items.Count(i => !i.IsDeleted && i.Status == ItemStatus.Evaluated),
                r.Items.Count(i => !i.IsDeleted && i.IsRejected),
                r.Notes,
                r.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(receptions);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateSupplierRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var errors = ValidateSupplier(req.Name, req.Email, req.PhoneNumber, req.TaxNumber, req.Initial);
        if (errors.Count > 0)
            return Results.BadRequest(new { errors });

        // Check unique initial
        var initialUpper = req.Initial.Trim().ToUpper();
        var initialExists = await db.Suppliers
            .AnyAsync(s => s.Initial.ToUpper() == initialUpper, ct);

        if (initialExists)
            return Results.Conflict(new { error = "Já existe um fornecedor com esta inicial." });

        // Check unique email
        var emailLower = req.Email.Trim().ToLower();
        var emailExists = await db.Suppliers
            .AnyAsync(s => s.Email.ToLower() == emailLower, ct);

        if (emailExists)
            return Results.Conflict(new { error = "Já existe um fornecedor com este email." });

        // Check unique NIF (if provided)
        if (!string.IsNullOrWhiteSpace(req.TaxNumber))
        {
            var nif = req.TaxNumber.Trim();
            var nifExists = await db.Suppliers
                .AnyAsync(s => s.TaxNumber != null && s.TaxNumber == nif, ct);

            if (nifExists)
                return Results.Conflict(new { error = "Já existe um fornecedor com este NIF." });
        }

        var supplier = new SupplierEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Email = req.Email.Trim().ToLower(),
            PhoneNumber = req.PhoneNumber.Trim(),
            TaxNumber = req.TaxNumber?.Trim(),
            Initial = initialUpper,
            Notes = req.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/suppliers/{supplier.ExternalId}",
            new SupplierDetailResponse(
                supplier.ExternalId,
                supplier.Name,
                supplier.Email,
                supplier.PhoneNumber,
                supplier.TaxNumber,
                supplier.Initial,
                supplier.Notes,
                0,
                supplier.CreatedOn,
                supplier.CreatedBy,
                supplier.UpdatedOn,
                supplier.UpdatedBy
            )
        );
    }

    private static async Task<IResult> Update(
        Guid externalId,
        [FromBody] UpdateSupplierRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        var errors = ValidateSupplier(req.Name, req.Email, req.PhoneNumber, req.TaxNumber, req.Initial);
        if (errors.Count > 0)
            return Results.BadRequest(new { errors });

        // Check unique initial (excluding current supplier)
        var initialUpper = req.Initial.Trim().ToUpper();
        var initialExists = await db.Suppliers
            .AnyAsync(s => s.Initial.ToUpper() == initialUpper && s.Id != supplier.Id, ct);

        if (initialExists)
            return Results.Conflict(new { error = "Já existe um fornecedor com esta inicial." });

        // Check unique email (excluding current supplier)
        var emailLower = req.Email.Trim().ToLower();
        var emailExists = await db.Suppliers
            .AnyAsync(s => s.Email.ToLower() == emailLower && s.Id != supplier.Id, ct);

        if (emailExists)
            return Results.Conflict(new { error = "Já existe um fornecedor com este email." });

        // Check unique NIF (if provided, excluding current supplier)
        if (!string.IsNullOrWhiteSpace(req.TaxNumber))
        {
            var nif = req.TaxNumber.Trim();
            var nifExists = await db.Suppliers
                .AnyAsync(s => s.TaxNumber != null && s.TaxNumber == nif && s.Id != supplier.Id, ct);

            if (nifExists)
                return Results.Conflict(new { error = "Já existe um fornecedor com este NIF." });
        }

        supplier.Name = req.Name.Trim();
        supplier.Email = req.Email.Trim().ToLower();
        supplier.PhoneNumber = req.PhoneNumber.Trim();
        supplier.TaxNumber = req.TaxNumber?.Trim();
        supplier.Initial = initialUpper;
        supplier.Notes = req.Notes?.Trim();
        supplier.UpdatedOn = DateTime.UtcNow;
        supplier.UpdatedBy = "system"; // TODO: Get from JWT claims

        await db.SaveChangesAsync(ct);

        var itemCount = await db.Items.CountAsync(i => i.SupplierId == supplier.Id && !i.IsDeleted, ct);

        return Results.Ok(new SupplierDetailResponse(
            supplier.ExternalId,
            supplier.Name,
            supplier.Email,
            supplier.PhoneNumber,
            supplier.TaxNumber,
            supplier.Initial,
            supplier.Notes,
            itemCount,
            supplier.CreatedOn,
            supplier.CreatedBy,
            supplier.UpdatedOn,
            supplier.UpdatedBy
        ));
    }

    private static async Task<IResult> Delete(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        // Check if supplier has non-deleted items
        var hasItems = await db.Items.AnyAsync(i => i.SupplierId == supplier.Id && !i.IsDeleted, ct);
        if (hasItems)
            return Results.Conflict(new { error = "Não é possível eliminar um fornecedor com itens associados." });

        // Check if supplier has non-deleted receptions
        var hasReceptions = await db.Receptions.AnyAsync(r => r.SupplierId == supplier.Id && !r.IsDeleted, ct);
        if (hasReceptions)
            return Results.Conflict(new { error = "Não é possível eliminar um fornecedor com recepções associadas." });

        // Soft delete
        supplier.IsDeleted = true;
        supplier.DeletedBy = "system"; // TODO: Get from JWT claims
        supplier.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    // ── Validation helpers ──

    private static Dictionary<string, string> ValidateSupplier(
        string name, string email, string phoneNumber, string? taxNumber, string initial)
    {
        var errors = new Dictionary<string, string>();

        // Name
        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = "O nome é obrigatório.";
        else if (name.Trim().Length > 256)
            errors["name"] = "O nome deve ter no máximo 256 caracteres.";

        // Email
        if (string.IsNullOrWhiteSpace(email))
            errors["email"] = "O email é obrigatório.";
        else if (!IsValidEmail(email.Trim()))
            errors["email"] = "O email não é válido.";

        // Phone (+351XXXXXXXXX)
        if (string.IsNullOrWhiteSpace(phoneNumber))
            errors["phoneNumber"] = "O telefone é obrigatório.";
        else if (!IsValidPortuguesePhone(phoneNumber.Trim()))
            errors["phoneNumber"] = "O telefone deve estar no formato +351XXXXXXXXX (9 dígitos após +351).";

        // NIF (optional but must be valid if provided)
        if (!string.IsNullOrWhiteSpace(taxNumber) && !IsValidPortugueseNif(taxNumber.Trim()))
            errors["taxNumber"] = "O NIF deve conter exatamente 9 dígitos e ser válido.";

        // Initial
        if (string.IsNullOrWhiteSpace(initial))
            errors["initial"] = "A inicial é obrigatória.";
        else if (initial.Trim().Length > 5)
            errors["initial"] = "A inicial deve ter no máximo 5 caracteres.";
        else if (!Regex.IsMatch(initial.Trim(), @"^[A-Za-z]+$"))
            errors["initial"] = "A inicial deve conter apenas letras.";

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPortuguesePhone(string phone)
    {
        // Format: +351XXXXXXXXX (9 digits after country code)
        return Regex.IsMatch(phone, @"^\+351\d{9}$");
    }

    private static bool IsValidPortugueseNif(string nif)
    {
        // Portuguese NIF: exactly 9 digits
        if (!Regex.IsMatch(nif, @"^\d{9}$"))
            return false;

        // Valid NIF first digits: 1, 2, 3, 5, 6, 7, 8, 9
        var firstDigit = nif[0];
        if (firstDigit == '0' || firstDigit == '4')
            return false;

        // Check digit validation (mod 11)
        var sum = 0;
        for (var i = 0; i < 8; i++)
        {
            sum += (nif[i] - '0') * (9 - i);
        }

        var remainder = sum % 11;
        var checkDigit = remainder < 2 ? 0 : 11 - remainder;

        return (nif[8] - '0') == checkDigit;
    }
}

// Request DTOs
public record CreateSupplierRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    string? Notes
);

public record UpdateSupplierRequest(
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    string? Notes
);

// Response DTOs
public record SupplierListResponse(
    Guid ExternalId,
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    int ItemCount,
    DateTime CreatedOn
);

public record SupplierDetailResponse(
    Guid ExternalId,
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    string? Notes,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);

public record SupplierItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    decimal EvaluatedPrice,
    string Status,
    string Condition,
    int DaysInStock,
    DateTime CreatedOn
);

public record SupplierReceptionResponse(
    Guid ExternalId,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    string? Notes,
    DateTime CreatedOn
);
