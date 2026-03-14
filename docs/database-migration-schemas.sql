-- =============================================================================
-- OUI System: Database Migration to Per-Module Schemas
-- =============================================================================
-- Run this script BEFORE deploying the new code with per-module DbContexts.
-- PostgreSQL supports cross-schema FK constraints natively, so existing FKs
-- will be preserved after the ALTER TABLE SET SCHEMA operations.
-- =============================================================================

BEGIN;

-- 1. Create schemas
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS sales;
CREATE SCHEMA IF NOT EXISTS ecommerce;
CREATE SCHEMA IF NOT EXISTS system;

-- 2. Move Auth tables
ALTER TABLE public."Users" SET SCHEMA auth;
ALTER TABLE public."Roles" SET SCHEMA auth;
ALTER TABLE public."Permissions" SET SCHEMA auth;
ALTER TABLE public."UserRoles" SET SCHEMA auth;
ALTER TABLE public."RolePermissions" SET SCHEMA auth;

-- 3. Move Inventory tables
ALTER TABLE public."Suppliers" SET SCHEMA inventory;
ALTER TABLE public."Brands" SET SCHEMA inventory;
ALTER TABLE public."Categories" SET SCHEMA inventory;
ALTER TABLE public."Tags" SET SCHEMA inventory;
ALTER TABLE public."Receptions" SET SCHEMA inventory;
ALTER TABLE public."Items" SET SCHEMA inventory;
ALTER TABLE public."ItemPhotos" SET SCHEMA inventory;
ALTER TABLE public."SupplierReturns" SET SCHEMA inventory;
ALTER TABLE public."ReceptionApprovalTokens" SET SCHEMA inventory;
ALTER TABLE public."ConsignmentItems" SET SCHEMA inventory;
ALTER TABLE public."ItemTags" SET SCHEMA inventory;

-- 4. Move Sales tables
ALTER TABLE public."CashRegisters" SET SCHEMA sales;
ALTER TABLE public."Sales" SET SCHEMA sales;
ALTER TABLE public."SaleItems" SET SCHEMA sales;
ALTER TABLE public."SalePayments" SET SCHEMA sales;
ALTER TABLE public."Settlements" SET SCHEMA sales;
ALTER TABLE public."StoreCredits" SET SCHEMA sales;
ALTER TABLE public."StoreCreditTransactions" SET SCHEMA sales;
ALTER TABLE public."SupplierCashBalanceTransactions" SET SCHEMA sales;

-- 5. Move Ecommerce tables
ALTER TABLE public."EcommerceProducts" SET SCHEMA ecommerce;
ALTER TABLE public."EcommerceProductPhotos" SET SCHEMA ecommerce;
ALTER TABLE public."EcommerceOrders" SET SCHEMA ecommerce;
ALTER TABLE public."EcommerceOrderItems" SET SCHEMA ecommerce;

-- 6. Move System tables
ALTER TABLE public."SystemSettings" SET SCHEMA system;

-- 7. Create per-schema migration history tables
CREATE TABLE IF NOT EXISTS auth."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);
CREATE TABLE IF NOT EXISTS inventory."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);
CREATE TABLE IF NOT EXISTS sales."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);
CREATE TABLE IF NOT EXISTS ecommerce."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);
CREATE TABLE IF NOT EXISTS system."__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL PRIMARY KEY,
    "ProductVersion" varchar(32) NOT NULL
);

-- 8. Insert baseline migration entries (timestamps match the Baseline migrations)
INSERT INTO auth."__EFMigrationsHistory" VALUES ('20260315000000_Baseline', '9.0.1');
INSERT INTO inventory."__EFMigrationsHistory" VALUES ('20260315000000_Baseline', '9.0.1');
INSERT INTO sales."__EFMigrationsHistory" VALUES ('20260315000000_Baseline', '9.0.1');
INSERT INTO ecommerce."__EFMigrationsHistory" VALUES ('20260315000000_Baseline', '9.0.1');
INSERT INTO system."__EFMigrationsHistory" VALUES ('20260315000000_Baseline', '9.0.1');

COMMIT;

-- =============================================================================
-- VERIFICATION QUERIES (run after migration)
-- =============================================================================

-- Check table counts per schema:
-- SELECT schemaname, count(*) FROM pg_tables
-- WHERE schemaname IN ('auth', 'inventory', 'sales', 'ecommerce', 'system')
-- GROUP BY schemaname ORDER BY schemaname;

-- Verify cross-schema FK constraints still exist:
-- SELECT conname, conrelid::regclass, confrelid::regclass
-- FROM pg_constraint WHERE contype = 'f'
-- AND (conrelid::regclass::text LIKE 'sales.%' OR conrelid::regclass::text LIKE 'ecommerce.%')
-- AND confrelid::regclass::text LIKE 'inventory.%';

-- Optional cleanup (after confirming everything works):
-- DROP TABLE IF EXISTS public."__EFMigrationsHistory";
