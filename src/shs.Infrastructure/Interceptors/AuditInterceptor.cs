using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Oui.Modules.System.Infrastructure;
using shs.Domain.Entities;

namespace shs.Infrastructure.Interceptors;

public sealed class AuditInterceptor(
    IHttpContextAccessor httpContextAccessor,
    SystemDbContext systemDb) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || eventData.Context is SystemDbContext)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var userEmail = httpContextAccessor.HttpContext?.User
            .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? httpContextAccessor.HttpContext?.User
            .FindFirst("email")?.Value;

        var now = DateTime.UtcNow;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLogEntity)
                continue;

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                auditEntries.Add(CreateAuditEntry(entry, userEmail, now));
            }
        }

        if (auditEntries.Count > 0)
        {
            var auditLogs = auditEntries.Select(e => e.ToAuditLog()).ToList();
            systemDb.AuditLogs.AddRange(auditLogs);
            await systemDb.SaveChangesAsync(cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static AuditEntry CreateAuditEntry(EntityEntry entry, string? userEmail, DateTime timestamp)
    {
        var auditEntry = new AuditEntry
        {
            EntityName = entry.Entity.GetType().Name,
            Action = entry.State.ToString(),
            UserEmail = userEmail,
            Timestamp = timestamp
        };

        var primaryKey = entry.Properties
            .FirstOrDefault(p => p.Metadata.IsPrimaryKey());

        auditEntry.EntityId = primaryKey?.CurrentValue?.ToString() ?? "";

        switch (entry.State)
        {
            case EntityState.Added:
                auditEntry.NewValues = entry.Properties
                    .Where(p => p.CurrentValue is not null)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                break;

            case EntityState.Deleted:
                auditEntry.OldValues = entry.Properties
                    .Where(p => p.OriginalValue is not null)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                break;

            case EntityState.Modified:
                var changedProperties = entry.Properties
                    .Where(p => p.IsModified)
                    .ToList();

                auditEntry.ChangedColumns = changedProperties
                    .Select(p => p.Metadata.Name)
                    .ToList();

                auditEntry.OldValues = changedProperties
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

                auditEntry.NewValues = changedProperties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                break;
        }

        return auditEntry;
    }

    private sealed class AuditEntry
    {
        public string EntityName { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public Dictionary<string, object?>? OldValues { get; set; }
        public Dictionary<string, object?>? NewValues { get; set; }
        public List<string>? ChangedColumns { get; set; }
        public string? UserEmail { get; set; }
        public DateTime Timestamp { get; set; }

        public AuditLogEntity ToAuditLog() => new()
        {
            EntityName = EntityName,
            EntityId = EntityId,
            Action = Action,
            OldValues = OldValues is not null ? JsonSerializer.Serialize(OldValues, JsonOptions) : null,
            NewValues = NewValues is not null ? JsonSerializer.Serialize(NewValues, JsonOptions) : null,
            ChangedColumns = ChangedColumns is not null ? JsonSerializer.Serialize(ChangedColumns, JsonOptions) : null,
            UserEmail = UserEmail,
            Timestamp = Timestamp
        };
    }
}
