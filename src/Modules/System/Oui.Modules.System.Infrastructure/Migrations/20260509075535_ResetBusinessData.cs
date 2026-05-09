using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oui.Modules.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResetBusinessData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset de dados de negócio (one-off pre-go-live).
            // Preserva: schema auth (completo), system.SystemSettings, __EFMigrationsHistory.
            // Wipa: inventory.*, sales.*, ecommerce.*, system.AuditLogs.
            // Roda APÓS as migrations dos outros DbContexts (ver Program.cs ordem),
            // garantindo que todos os schemas/tabelas alvo já existem.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    r RECORD;
                    target_schemas TEXT[] := ARRAY['inventory', 'sales', 'ecommerce'];
                BEGIN
                    FOR r IN
                        SELECT schemaname, tablename
                        FROM pg_tables
                        WHERE schemaname = ANY(target_schemas)
                          AND tablename <> '__EFMigrationsHistory'
                    LOOP
                        EXECUTE format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE',
                                       r.schemaname, r.tablename);
                    END LOOP;
                END $$;
            ");

            migrationBuilder.Sql(@"TRUNCATE TABLE system.""AuditLogs"" RESTART IDENTITY CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversível: dados truncados não podem ser restaurados via rollback.
        }
    }
}
