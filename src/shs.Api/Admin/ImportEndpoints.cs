using Microsoft.AspNetCore.Mvc;
using shs.Api.Authorization;
using shs.Infrastructure.Services.Import;

namespace shs.Api.Admin;

public static class ImportEndpoints
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB
    private const string ExpectedMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/import").WithTags("Import");

        group.MapPost("/personal-items", ImportPersonalItems)
            .RequirePermission("admin.import.execute")
            .DisableAntiforgery();

        group.MapPost("/consignment-items", ImportConsignmentItems)
            .RequirePermission("admin.import.execute")
            .DisableAntiforgery();
    }

    private static async Task<IResult> ImportPersonalItems(
        [FromForm] IFormFile file,
        [FromServices] ImportService importService,
        CancellationToken ct)
    {
        var validationError = ValidateFile(file);
        if (validationError is not null)
            return validationError;

        using var stream = file.OpenReadStream();
        var result = await importService.ImportPersonalItemsAsync(stream, ct);

        return Results.Ok(new ImportResultResponse(
            RowsRead: result.EstoqueRowsRead,
            BrandsCreated: result.BrandsCreated,
            SuppliersCreated: result.SuppliersCreated,
            ItemsImported: result.ItemsFromEstoque,
            Errors: result.Errors,
            ErrorDetails: result.ErrorDetails));
    }

    private static async Task<IResult> ImportConsignmentItems(
        [FromForm] IFormFile file,
        [FromServices] ImportService importService,
        CancellationToken ct)
    {
        var validationError = ValidateFile(file);
        if (validationError is not null)
            return validationError;

        using var stream = file.OpenReadStream();
        var result = await importService.ImportConsignmentItemsAsync(stream, ct);

        return Results.Ok(new ImportResultResponse(
            RowsRead: result.ConsignadoRowsRead,
            BrandsCreated: result.BrandsCreated,
            SuppliersCreated: result.SuppliersCreated,
            ItemsImported: result.ItemsFromConsignados,
            Errors: result.Errors,
            ErrorDetails: result.ErrorDetails));
    }

    private static IResult? ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            return Results.BadRequest(new { error = "Arquivo vazio." });

        if (file.Length > MaxFileSize)
            return Results.BadRequest(new { error = "Arquivo excede o tamanho máximo de 20 MB." });

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Formato inválido. Apenas arquivos .xlsx são aceitos." });

        if (file.ContentType != ExpectedMimeType)
            return Results.BadRequest(new { error = "MIME type inválido. Esperado: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

        return null;
    }

    private record ImportResultResponse(
        int RowsRead,
        int BrandsCreated,
        int SuppliersCreated,
        int ItemsImported,
        int Errors,
        List<string> ErrorDetails);
}
