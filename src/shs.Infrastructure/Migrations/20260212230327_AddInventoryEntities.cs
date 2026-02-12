using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace shs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_ConsignmentItems_ConsignmentItemId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "ConsignmentItemId",
                table: "SaleItems",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ConsignmentItemId",
                table: "SaleItems",
                newName: "IX_SaleItems_ItemId");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParentCategoryId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TaxNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Initial = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
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
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Receptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<long>(type: "bigint", nullable: false),
                    ReceptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvaluatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_Receptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receptions_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentificationNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Composition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvaluatedPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalSalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AcquisitionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Origin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplierId = table.Column<long>(type: "bigint", nullable: true),
                    ReceptionId = table.Column<long>(type: "bigint", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsRejected = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SaleId = table.Column<long>(type: "bigint", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysInStock = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Receptions_ReceptionId",
                        column: x => x.ReceptionId,
                        principalTable: "Receptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPhotos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_ItemPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPhotos_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemTags",
                columns: table => new
                {
                    ItemsId = table.Column<long>(type: "bigint", nullable: false),
                    TagsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTags", x => new { x.ItemsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ItemTags_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brands_ExternalId",
                table: "Brands",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ExternalId",
                table: "Categories",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ExternalId",
                table: "ItemPhotos",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId_DisplayOrder",
                table: "ItemPhotos",
                columns: new[] { "ItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_AcquisitionType",
                table: "Items",
                column: "AcquisitionType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BrandId",
                table: "Items",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                table: "Items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Color",
                table: "Items",
                column: "Color");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Condition",
                table: "Items",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DaysInStock",
                table: "Items",
                column: "DaysInStock");

            migrationBuilder.CreateIndex(
                name: "IX_Items_EvaluatedPrice",
                table: "Items",
                column: "EvaluatedPrice");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ExternalId",
                table: "Items",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_IdentificationNumber",
                table: "Items",
                column: "IdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Origin",
                table: "Items",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ReceptionId",
                table: "Items",
                column: "ReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SaleId",
                table: "Items",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Size",
                table: "Items",
                column: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Status",
                table: "Items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SupplierId",
                table: "Items",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTags_TagsId",
                table: "ItemTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_ExternalId",
                table: "Receptions",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_ReceptionDate",
                table: "Receptions",
                column: "ReceptionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_Status",
                table: "Receptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_SupplierId",
                table: "Receptions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Email",
                table: "Suppliers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ExternalId",
                table: "Suppliers",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Initial",
                table: "Suppliers",
                column: "Initial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TaxNumber",
                table: "Suppliers",
                column: "TaxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ExternalId",
                table: "Tags",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Items_ItemId",
                table: "SaleItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Seed data for Brands
            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "ExternalId", "Name", "Description", "LogoUrl", "IsDeleted", "CreatedOn" },
                values: new object[,]
                {
                    { 1L, Guid.NewGuid(), "Zara", "Fashion and quality at the best price", null, false, DateTime.UtcNow },
                    { 2L, Guid.NewGuid(), "H&M", "Fashion and quality at the best price", null, false, DateTime.UtcNow },
                    { 3L, Guid.NewGuid(), "Mango", "Mediterranean-inspired fashion", null, false, DateTime.UtcNow },
                    { 4L, Guid.NewGuid(), "Massimo Dutti", "Contemporary, timeless fashion", null, false, DateTime.UtcNow },
                    { 5L, Guid.NewGuid(), "Bershka", "Young, fresh fashion", null, false, DateTime.UtcNow },
                    { 6L, Guid.NewGuid(), "Pull & Bear", "Casual urban fashion", null, false, DateTime.UtcNow },
                    { 7L, Guid.NewGuid(), "Stradivarius", "Fashion for young women", null, false, DateTime.UtcNow },
                    { 8L, Guid.NewGuid(), "Oysho", "Lingerie and sportswear", null, false, DateTime.UtcNow },
                    { 9L, Guid.NewGuid(), "Uterqüe", "Premium accessories and fashion", null, false, DateTime.UtcNow },
                    { 10L, Guid.NewGuid(), "Cos", "Modern, functional, timeless design", null, false, DateTime.UtcNow },
                    { 11L, Guid.NewGuid(), "Uniqlo", "Simple, quality everyday wear", null, false, DateTime.UtcNow },
                    { 12L, Guid.NewGuid(), "Primark", "Affordable fashion for all", null, false, DateTime.UtcNow },
                    { 13L, Guid.NewGuid(), "Reserved", "European trendy fashion", null, false, DateTime.UtcNow },
                    { 14L, Guid.NewGuid(), "Outro", "Other brands", null, false, DateTime.UtcNow }
                });

            // Seed data for Categories
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ExternalId", "Name", "Description", "ParentCategoryId", "IsDeleted", "CreatedOn" },
                values: new object[,]
                {
                    { 1L, Guid.NewGuid(), "Vestuário", "Roupas em geral", null, false, DateTime.UtcNow },
                    { 2L, Guid.NewGuid(), "Calçado", "Sapatos e acessórios para os pés", null, false, DateTime.UtcNow },
                    { 3L, Guid.NewGuid(), "Acessórios", "Bolsas, cintos, joias", null, false, DateTime.UtcNow }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ExternalId", "Name", "Description", "ParentCategoryId", "IsDeleted", "CreatedOn" },
                values: new object[,]
                {
                    { 4L, Guid.NewGuid(), "Vestidos", "Vestidos de todos os estilos", 1L, false, DateTime.UtcNow },
                    { 5L, Guid.NewGuid(), "Calças", "Calças de ganga, leggings, etc.", 1L, false, DateTime.UtcNow },
                    { 6L, Guid.NewGuid(), "Casacos", "Casacos e jaquetas", 1L, false, DateTime.UtcNow },
                    { 7L, Guid.NewGuid(), "Tops e Camisolas", "Camisas, blusas, t-shirts", 1L, false, DateTime.UtcNow },
                    { 8L, Guid.NewGuid(), "Saias", "Saias de vários comprimentos", 1L, false, DateTime.UtcNow },
                    { 9L, Guid.NewGuid(), "Conjuntos", "Conjuntos coordenados", 1L, false, DateTime.UtcNow },
                    { 10L, Guid.NewGuid(), "Roupa Interior", "Lingerie e roupa de dormir", 1L, false, DateTime.UtcNow },
                    { 11L, Guid.NewGuid(), "Desportivo", "Roupa desportiva e ativa", 1L, false, DateTime.UtcNow },
                    { 12L, Guid.NewGuid(), "Sapatos", "Sapatos casuais", 2L, false, DateTime.UtcNow },
                    { 13L, Guid.NewGuid(), "Botas", "Botas de todos os tipos", 2L, false, DateTime.UtcNow },
                    { 14L, Guid.NewGuid(), "Sandálias", "Sandálias e chinelos", 2L, false, DateTime.UtcNow },
                    { 15L, Guid.NewGuid(), "Sapatilhas", "Ténis e sapatilhas", 2L, false, DateTime.UtcNow },
                    { 16L, Guid.NewGuid(), "Bolsas", "Malas de mão e carteiras", 3L, false, DateTime.UtcNow },
                    { 17L, Guid.NewGuid(), "Cintos", "Cintos e suspensórios", 3L, false, DateTime.UtcNow },
                    { 18L, Guid.NewGuid(), "Joias", "Colares, pulseiras, brincos", 3L, false, DateTime.UtcNow },
                    { 19L, Guid.NewGuid(), "Lenços e Cachecóis", "Lenços, cachecóis, echarpes", 3L, false, DateTime.UtcNow }
                });

            // Seed data for Tags
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "ExternalId", "Name", "Color", "IsDeleted", "CreatedOn" },
                values: new object[,]
                {
                    { 1L, Guid.NewGuid(), "Verão", "#FFA500", false, DateTime.UtcNow },
                    { 2L, Guid.NewGuid(), "Inverno", "#4169E1", false, DateTime.UtcNow },
                    { 3L, Guid.NewGuid(), "Primavera", "#90EE90", false, DateTime.UtcNow },
                    { 4L, Guid.NewGuid(), "Outono", "#D2691E", false, DateTime.UtcNow },
                    { 5L, Guid.NewGuid(), "Formal", "#000000", false, DateTime.UtcNow },
                    { 6L, Guid.NewGuid(), "Casual", "#808080", false, DateTime.UtcNow },
                    { 7L, Guid.NewGuid(), "Desportivo", "#FF4500", false, DateTime.UtcNow },
                    { 8L, Guid.NewGuid(), "Festa", "#FFD700", false, DateTime.UtcNow },
                    { 9L, Guid.NewGuid(), "Praia", "#00CED1", false, DateTime.UtcNow },
                    { 10L, Guid.NewGuid(), "Vintage", "#8B4513", false, DateTime.UtcNow },
                    { 11L, Guid.NewGuid(), "Sustentável", "#228B22", false, DateTime.UtcNow },
                    { 12L, Guid.NewGuid(), "Premium", "#9370DB", false, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Items_ItemId",
                table: "SaleItems");

            migrationBuilder.DropTable(
                name: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "ItemTags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Receptions");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "SaleItems",
                newName: "ConsignmentItemId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ItemId",
                table: "SaleItems",
                newName: "IX_SaleItems_ConsignmentItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_ConsignmentItems_ConsignmentItemId",
                table: "SaleItems",
                column: "ConsignmentItemId",
                principalTable: "ConsignmentItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
