using shs.Domain.Results;

namespace Oui.Modules.System.Application.AuditLogs;

public static class AuditLogErrors
{
    public static readonly Error InvalidDateRange = Error.Problem(
        "AuditLog.InvalidDateRange", "dateFrom must be before dateTo");
}
