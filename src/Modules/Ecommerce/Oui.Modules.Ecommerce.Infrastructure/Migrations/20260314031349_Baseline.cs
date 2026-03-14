using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oui.Modules.Ecommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ecommerce");

            migrationBuilder.CreateTable(
                name: "EcommerceOrders",
                schema: "ecommerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcommerceOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EcommerceProducts",
                schema: "ecommerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Composition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnpublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcommerceProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EcommerceOrderItems",
                schema: "ecommerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    ProductTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcommerceOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcommerceOrderItems_EcommerceOrders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "ecommerce",
                        principalTable: "EcommerceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EcommerceOrderItems_EcommerceProducts_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "ecommerce",
                        principalTable: "EcommerceProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcommerceProductPhotos",
                schema: "ecommerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ThumbnailPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcommerceProductPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcommerceProductPhotos_EcommerceProducts_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "ecommerce",
                        principalTable: "EcommerceProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrderItems_ExternalId",
                schema: "ecommerce",
                table: "EcommerceOrderItems",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrderItems_ItemId",
                schema: "ecommerce",
                table: "EcommerceOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrderItems_OrderId",
                schema: "ecommerce",
                table: "EcommerceOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrderItems_ProductId",
                schema: "ecommerce",
                table: "EcommerceOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrders_CustomerEmail",
                schema: "ecommerce",
                table: "EcommerceOrders",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrders_ExpiresAt",
                schema: "ecommerce",
                table: "EcommerceOrders",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrders_ExternalId",
                schema: "ecommerce",
                table: "EcommerceOrders",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrders_OrderNumber",
                schema: "ecommerce",
                table: "EcommerceOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceOrders_Status",
                schema: "ecommerce",
                table: "EcommerceOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProductPhotos_ExternalId",
                schema: "ecommerce",
                table: "EcommerceProductPhotos",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProductPhotos_ProductId_DisplayOrder",
                schema: "ecommerce",
                table: "EcommerceProductPhotos",
                columns: new[] { "ProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_BrandName",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_CategoryName",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_ExternalId",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_ItemId",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_Price",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_Slug",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcommerceProducts_Status",
                schema: "ecommerce",
                table: "EcommerceProducts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EcommerceOrderItems",
                schema: "ecommerce");

            migrationBuilder.DropTable(
                name: "EcommerceProductPhotos",
                schema: "ecommerce");

            migrationBuilder.DropTable(
                name: "EcommerceOrders",
                schema: "ecommerce");

            migrationBuilder.DropTable(
                name: "EcommerceProducts",
                schema: "ecommerce");
        }
    }
}
