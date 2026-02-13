using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace shs.Infrastructure.Services;

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "OUI System";
    public bool UseSsl { get; set; } = true;
    public bool Enabled { get; set; } = true;
}

public interface IEmailService
{
    Task SendEvaluationEmailAsync(EvaluationEmailData data, CancellationToken ct = default);
}

public class EvaluationEmailData
{
    public required string SupplierName { get; init; }
    public required string SupplierEmail { get; init; }
    public required DateTime ReceptionDate { get; init; }
    public required string ReceptionRef { get; init; }
    public required List<EvaluationEmailItem> AcceptedItems { get; init; }
    public required List<EvaluationEmailItem> RejectedItems { get; init; }
}

public class EvaluationEmailItem
{
    public required string IdentificationNumber { get; init; }
    public required string Name { get; init; }
    public required string Brand { get; init; }
    public required string Size { get; init; }
    public required string Color { get; init; }
    public required string Condition { get; init; }
    public required decimal EvaluatedPrice { get; init; }
    public string? RejectionReason { get; init; }
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEvaluationEmailAsync(EvaluationEmailData data, CancellationToken ct = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogWarning("Email sending is disabled. Skipping evaluation email to {Email}", data.SupplierEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(data.SupplierName, data.SupplierEmail));
        message.Subject = $"OUI - Resultado da Avalia\u00e7\u00e3o de Pe\u00e7as (Ref: {data.ReceptionRef})";

        var htmlBody = BuildEvaluationEmailHtml(data);
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                ct);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation(
                "Evaluation email sent successfully to {Email} for reception {Ref}",
                data.SupplierEmail, data.ReceptionRef);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send evaluation email to {Email} for reception {Ref}",
                data.SupplierEmail, data.ReceptionRef);
            throw;
        }
    }

    private static string E(string? s) => System.Net.WebUtility.HtmlEncode(s ?? "");

    private static string BuildEvaluationEmailHtml(EvaluationEmailData data)
    {
        var supplierName = E(data.SupplierName);
        var receptionDate = data.ReceptionDate.ToString("dd/MM/yyyy");
        var receptionRef = E(data.ReceptionRef);
        var totalAccepted = data.AcceptedItems.Count;
        var totalRejected = data.RejectedItems.Count;
        var totalItems = totalAccepted + totalRejected;

        // Build accepted items rows
        var acceptedRows = "";
        foreach (var item in data.AcceptedItems)
        {
            acceptedRows += "<tr>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;font-family:monospace;font-size:12px;color:#64748b;\">" + E(item.IdentificationNumber) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;font-weight:600;\">" + E(item.Name) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;\">" + E(item.Brand) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;\">" + E(item.Size) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;\">" + E(item.Color) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;\">" + GetConditionLabel(item.Condition) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #e2e8f0;font-weight:600;text-align:right;\">" + item.EvaluatedPrice.ToString("C", new System.Globalization.CultureInfo("pt-PT")) + "</td>"
                + "</tr>";
        }

        // Build rejected items rows
        var rejectedRows = "";
        foreach (var item in data.RejectedItems)
        {
            rejectedRows += "<tr>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #fecaca;font-family:monospace;font-size:12px;color:#64748b;\">" + E(item.IdentificationNumber) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #fecaca;font-weight:600;\">" + E(item.Name) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #fecaca;\">" + E(item.Brand) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #fecaca;\">" + E(item.Size) + "</td>"
                + "<td style=\"padding:8px 12px;border-bottom:1px solid #fecaca;\">" + (string.IsNullOrEmpty(item.RejectionReason) ? "\u2014" : E(item.RejectionReason)) + "</td>"
                + "</tr>";
        }

        // Rejected section (only if there are rejected items)
        var rejectedSection = totalRejected > 0
            ? "<div style=\"margin-top:28px;\">"
              + "<h2 style=\"font-size:16px;font-weight:700;color:#991b1b;margin:0 0 12px;\">Pe\u00e7as Recusadas (" + totalRejected + ")</h2>"
              + "<p style=\"font-size:13px;color:#64748b;margin:0 0 12px;\">Estas pe\u00e7as n\u00e3o foram aceites e podem ser levantadas na loja.</p>"
              + "<table style=\"width:100%;border-collapse:collapse;font-size:13px;color:#1e293b;\">"
              + "<thead><tr style=\"background:#fef2f2;\">"
              + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#991b1b;border-bottom:1px solid #fecaca;\">ID</th>"
              + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#991b1b;border-bottom:1px solid #fecaca;\">Nome</th>"
              + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#991b1b;border-bottom:1px solid #fecaca;\">Marca</th>"
              + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#991b1b;border-bottom:1px solid #fecaca;\">Tam.</th>"
              + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#991b1b;border-bottom:1px solid #fecaca;\">Motivo</th>"
              + "</tr></thead><tbody>" + rejectedRows + "</tbody></table>"
              + "</div>"
            : "";

        // Build the full email
        return "<!DOCTYPE html><html lang=\"pt\"><head><meta charset=\"UTF-8\"></head>"
            + "<body style=\"margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;\">"
            + "<div style=\"max-width:640px;margin:0 auto;padding:32px 16px;\">"

            // Header
            + "<div style=\"background:#6366f1;color:white;padding:24px 32px;border-radius:12px 12px 0 0;text-align:center;\">"
            + "<h1 style=\"margin:0;font-size:20px;font-weight:700;\">OUI System</h1>"
            + "<p style=\"margin:4px 0 0;font-size:13px;opacity:0.85;\">Resultado da Avalia\u00e7\u00e3o de Pe\u00e7as</p>"
            + "</div>"

            // Body
            + "<div style=\"background:white;padding:32px;border-radius:0 0 12px 12px;border:1px solid #e2e8f0;border-top:none;\">"

            // Greeting
            + "<p style=\"font-size:15px;color:#1e293b;margin:0 0 20px;\">Ol\u00e1 <b>" + supplierName + "</b>,</p>"
            + "<p style=\"font-size:14px;color:#475569;margin:0 0 24px;line-height:1.6;\">"
            + "A avalia\u00e7\u00e3o das pe\u00e7as que entregou no dia <b>" + receptionDate + "</b> "
            + "(Ref: <b>" + receptionRef + "</b>) foi conclu\u00edda. "
            + "Segue abaixo o resumo do resultado."
            + "</p>"

            // Summary
            + "<div style=\"display:flex;gap:12px;margin-bottom:24px;\">"
            + "<div style=\"flex:1;background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:16px;text-align:center;\">"
            + "<div style=\"font-size:28px;font-weight:700;color:#16a34a;\">" + totalAccepted + "</div>"
            + "<div style=\"font-size:12px;color:#166534;margin-top:2px;\">Aceites</div>"
            + "</div>"
            + "<div style=\"flex:1;background:" + (totalRejected > 0 ? "#fef2f2;border:1px solid #fecaca" : "#f8fafc;border:1px solid #e2e8f0") + ";border-radius:8px;padding:16px;text-align:center;\">"
            + "<div style=\"font-size:28px;font-weight:700;color:" + (totalRejected > 0 ? "#dc2626" : "#94a3b8") + ";\">" + totalRejected + "</div>"
            + "<div style=\"font-size:12px;color:" + (totalRejected > 0 ? "#991b1b" : "#64748b") + ";margin-top:2px;\">Recusados</div>"
            + "</div>"
            + "<div style=\"flex:1;background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:16px;text-align:center;\">"
            + "<div style=\"font-size:28px;font-weight:700;color:#475569;\">" + totalItems + "</div>"
            + "<div style=\"font-size:12px;color:#64748b;margin-top:2px;\">Total</div>"
            + "</div>"
            + "</div>"

            // Accepted items table
            + (totalAccepted > 0
                ? "<h2 style=\"font-size:16px;font-weight:700;color:#166534;margin:0 0 12px;\">Pe\u00e7as Aceites (" + totalAccepted + ")</h2>"
                  + "<p style=\"font-size:13px;color:#64748b;margin:0 0 12px;\">Estas pe\u00e7as foram aceites e ser\u00e3o colocadas \u00e0 venda na loja.</p>"
                  + "<table style=\"width:100%;border-collapse:collapse;font-size:13px;color:#1e293b;\">"
                  + "<thead><tr style=\"background:#f0fdf4;\">"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">ID</th>"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Nome</th>"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Marca</th>"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Tam.</th>"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Cor</th>"
                  + "<th style=\"padding:8px 12px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Cond.</th>"
                  + "<th style=\"padding:8px 12px;text-align:right;font-size:11px;text-transform:uppercase;letter-spacing:0.5px;color:#166534;border-bottom:1px solid #bbf7d0;\">Pre\u00e7o</th>"
                  + "</tr></thead><tbody>" + acceptedRows + "</tbody></table>"
                : "")

            // Rejected items
            + rejectedSection

            // Footer note
            + "<div style=\"margin-top:32px;padding:16px;background:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;\">"
            + "<p style=\"font-size:13px;color:#64748b;margin:0;line-height:1.6;\">"
            + "<b>Nota:</b> As pe\u00e7as aceites ser\u00e3o colocadas \u00e0 venda na loja. "
            + "Ap\u00f3s a venda, ser\u00e1 emitido o acerto financeiro de acordo com a comiss\u00e3o acordada."
            + (totalRejected > 0
                ? " As pe\u00e7as recusadas podem ser levantadas na loja a qualquer momento."
                : "")
            + "</p>"
            + "</div>"

            + "<p style=\"font-size:13px;color:#94a3b8;margin:24px 0 0;text-align:center;\">"
            + "Obrigado pela confian\u00e7a! \u2014 Equipa OUI"
            + "</p>"
            + "</div>"

            // Email footer
            + "<div style=\"text-align:center;padding:16px;\">"
            + "<p style=\"font-size:11px;color:#94a3b8;margin:0;\">Este email foi gerado automaticamente pelo OUI System.</p>"
            + "<p style=\"font-size:11px;color:#94a3b8;margin:4px 0 0;\">Por favor n\u00e3o responda a este email.</p>"
            + "</div>"

            + "</div>"
            + "</body></html>";
    }

    private static string GetConditionLabel(string condition)
    {
        return condition switch
        {
            "Excellent" => "Excelente",
            "VeryGood" => "Muito Bom",
            "Good" => "Bom",
            "Fair" => "Razo\u00e1vel",
            "Poor" => "Mau",
            _ => condition
        };
    }
}
