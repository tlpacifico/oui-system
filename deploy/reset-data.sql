-- Reset de dados de negócio.
-- Preserva: schema auth (completo), system.SystemSettings, __EFMigrationsHistory de todos os schemas.
-- Wipa: inventory.*, sales.*, ecommerce.*, system.AuditLogs.
--
-- Execução (na VPS):
--   docker compose stop oui-system
--   psql "host=localhost port=5432 dbname=oui_db user=oui_user password=..." -f deploy/reset-data.sql
--   docker compose start oui-system

BEGIN;

-- 1) Trunca todas as tabelas dos schemas de negócio (exceto migrations history)
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
        RAISE NOTICE 'Truncated %.%', r.schemaname, r.tablename;
    END LOOP;
END $$;

-- 2) Trunca apenas AuditLogs do schema system (preserva SystemSettings)
TRUNCATE TABLE system."AuditLogs" RESTART IDENTITY CASCADE;

COMMIT;

-- Verificação manual sugerida após executar:
--   SELECT schemaname, tablename,
--          (xpath('/row/c/text()', query_to_xml(format('SELECT count(*) AS c FROM %I.%I', schemaname, tablename), false, true, '')))[1]::text::int AS row_count
--   FROM pg_tables
--   WHERE schemaname IN ('auth','inventory','sales','ecommerce','system')
--   ORDER BY schemaname, tablename;
