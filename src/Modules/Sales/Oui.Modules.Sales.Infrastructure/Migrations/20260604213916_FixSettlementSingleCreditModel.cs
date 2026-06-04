using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oui.Modules.Sales.Infrastructure.Migrations
{
    /// <summary>
    /// Data fix: o modelo antigo somava crédito em loja + resgate em dinheiro
    /// (NetAmountToSupplier = StoreCreditAmount + CashRedemptionAmount), tratando
    /// como cumulativo o que são alternativas. No modelo corrigido o fornecedor
    /// recebe UM saldo único em crédito (NetAmountToSupplier = StoreCreditAmount)
    /// e o resgate em dinheiro converte esse crédito (PorcInDinheiro/PorcInLoja).
    /// Remove também os créditos do saldo paralelo em dinheiro emitidos por acertos.
    /// </summary>
    public partial class FixSettlementSingleCreditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE sales."Settlements"
                SET "NetAmountToSupplier" = "StoreCreditAmount",
                    "StoreCommissionAmount" = "TotalSalesAmount" - "StoreCreditAmount"
                WHERE "NetAmountToSupplier" <> "StoreCreditAmount";
                """);

            // Remove os créditos de acerto do ledger paralelo (SettlementCredit).
            // O ledger passa a registar apenas resgates em dinheiro (auditoria).
            migrationBuilder.Sql(
                """
                DELETE FROM sales."SupplierCashBalanceTransactions"
                WHERE "TransactionType" = 'SettlementCredit';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Correção de dados irreversível (ambiente de teste).
        }
    }
}
