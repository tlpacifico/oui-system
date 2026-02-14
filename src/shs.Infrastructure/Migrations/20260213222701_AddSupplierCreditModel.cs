using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace shs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierCreditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashRedemptionPercentage",
                table: "Suppliers",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 40m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditPercentageInStore",
                table: "Suppliers",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 50m);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercentage",
                table: "Settlements",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "CashRedemptionAmount",
                table: "Settlements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashRedemptionPercentage",
                table: "Settlements",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditPercentageInStore",
                table: "Settlements",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StoreCreditAmount",
                table: "Settlements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Migrate existing settlements: PaymentMethod 1=Cash (40%), 2=StoreCredit (50%)
            migrationBuilder.Sql(@"
                UPDATE ""Settlements""
                SET ""CreditPercentageInStore"" = CASE WHEN ""PaymentMethod"" = 2 THEN 50 ELSE 0 END,
                    ""CashRedemptionPercentage"" = CASE WHEN ""PaymentMethod"" = 1 THEN 40 ELSE 0 END,
                    ""StoreCreditAmount"" = CASE WHEN ""PaymentMethod"" = 2 THEN ""NetAmountToSupplier"" ELSE 0 END,
                    ""CashRedemptionAmount"" = CASE WHEN ""PaymentMethod"" = 1 THEN ""NetAmountToSupplier"" ELSE 0 END
                WHERE ""CommissionPercentage"" IS NOT NULL OR ""PaymentMethod"" IN (1, 2);
            ");

            migrationBuilder.AddColumn<long>(
                name: "StoreCreditId",
                table: "SalePayments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SupplierId",
                table: "SalePayments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierCashBalanceTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SettlementId = table.Column<long>(type: "bigint", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCashBalanceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCashBalanceTransactions_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierCashBalanceTransactions_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_StoreCreditId",
                table: "SalePayments",
                column: "StoreCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_SupplierId",
                table: "SalePayments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCashBalanceTransactions_ExternalId",
                table: "SupplierCashBalanceTransactions",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCashBalanceTransactions_SettlementId",
                table: "SupplierCashBalanceTransactions",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCashBalanceTransactions_SupplierId",
                table: "SupplierCashBalanceTransactions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCashBalanceTransactions_TransactionDate",
                table: "SupplierCashBalanceTransactions",
                column: "TransactionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_SalePayments_StoreCredits_StoreCreditId",
                table: "SalePayments",
                column: "StoreCreditId",
                principalTable: "StoreCredits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalePayments_Suppliers_SupplierId",
                table: "SalePayments",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalePayments_StoreCredits_StoreCreditId",
                table: "SalePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SalePayments_Suppliers_SupplierId",
                table: "SalePayments");

            migrationBuilder.DropTable(
                name: "SupplierCashBalanceTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SalePayments_StoreCreditId",
                table: "SalePayments");

            migrationBuilder.DropIndex(
                name: "IX_SalePayments_SupplierId",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "CashRedemptionPercentage",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CreditPercentageInStore",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CashRedemptionAmount",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "CashRedemptionPercentage",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "CreditPercentageInStore",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "StoreCreditAmount",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "StoreCreditId",
                table: "SalePayments");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "SalePayments");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercentage",
                table: "Settlements",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
