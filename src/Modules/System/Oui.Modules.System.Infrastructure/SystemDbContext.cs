using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace Oui.Modules.System.Infrastructure;

public sealed class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options)
{
    public DbSet<SystemSettingEntity> SystemSettings => Set<SystemSettingEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.System);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
    }
}
