using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionReceipt;

internal sealed class GetReceptionReceiptQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReceptionReceiptQuery, string>
{
    public async Task<Result<string>> Handle(
        GetReceptionReceiptQuery request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<string>(ConsignmentErrors.ReceptionNotFound);

        return GenerateReceiptHtml(reception);
    }

    private static string GenerateReceiptHtml(ReceptionEntity reception)
    {
        var receptionDate = reception.ReceptionDate.ToString("dd/MM/yyyy HH:mm");
        var supplierName = global::System.Net.WebUtility.HtmlEncode(reception.Supplier.Name);
        var supplierNif = global::System.Net.WebUtility.HtmlEncode(reception.Supplier.TaxNumber ?? "\u2014");
        var itemCount = reception.ItemCount;
        var notes = global::System.Net.WebUtility.HtmlEncode(reception.Notes ?? "");
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
