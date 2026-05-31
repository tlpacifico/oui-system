using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oui.Modules.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colors",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HexCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
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
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemColors",
                schema: "inventory",
                columns: table => new
                {
                    ColorsId = table.Column<long>(type: "bigint", nullable: false),
                    ItemsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemColors", x => new { x.ColorsId, x.ItemsId });
                    table.ForeignKey(
                        name: "FK_ItemColors_Colors_ColorsId",
                        column: x => x.ColorsId,
                        principalSchema: "inventory",
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemColors_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalSchema: "inventory",
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colors_ExternalId",
                schema: "inventory",
                table: "Colors",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colors_Name",
                schema: "inventory",
                table: "Colors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemColors_ItemsId",
                schema: "inventory",
                table: "ItemColors",
                column: "ItemsId");

            // Seed base color palette (deterministic GUIDs/timestamp so the migration is reproducible)
            var seededOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var palette = new (string Name, string Hex, string ExternalId)[]
            {
                ("Preto", "#1C1917", "a1000001-0000-4000-8000-000000000001"),
                ("Branco", "#FFFFFF", "a1000001-0000-4000-8000-000000000002"),
                ("Cinzento", "#9CA3AF", "a1000001-0000-4000-8000-000000000003"),
                ("Bege", "#D9C3A5", "a1000001-0000-4000-8000-000000000004"),
                ("Castanho", "#6F4E37", "a1000001-0000-4000-8000-000000000005"),
                ("Azul", "#2563EB", "a1000001-0000-4000-8000-000000000006"),
                ("Azul Marinho", "#1E3A5F", "a1000001-0000-4000-8000-000000000007"),
                ("Vermelho", "#DC2626", "a1000001-0000-4000-8000-000000000008"),
                ("Rosa", "#EC4899", "a1000001-0000-4000-8000-000000000009"),
                ("Verde", "#16A34A", "a1000001-0000-4000-8000-00000000000a"),
                ("Verde Militar", "#4B5320", "a1000001-0000-4000-8000-00000000000b"),
                ("Amarelo", "#EAB308", "a1000001-0000-4000-8000-00000000000c"),
                ("Laranja", "#F97316", "a1000001-0000-4000-8000-00000000000d"),
                ("Roxo", "#7C3AED", "a1000001-0000-4000-8000-00000000000e"),
                ("Bordô", "#7B1E2B", "a1000001-0000-4000-8000-00000000000f"),
                ("Dourado", "#C9A227", "a1000001-0000-4000-8000-000000000010"),
                ("Prateado", "#C0C0C0", "a1000001-0000-4000-8000-000000000011"),
                ("Multicolor", "#A855F7", "a1000001-0000-4000-8000-000000000012"),
            };

            foreach (var c in palette)
            {
                migrationBuilder.InsertData(
                    schema: "inventory",
                    table: "Colors",
                    columns: new[] { "Name", "HexCode", "ExternalId", "CreatedOn", "CreatedBy", "IsDeleted" },
                    values: new object[] { c.Name, c.Hex, new Guid(c.ExternalId), seededOn, "system", false });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemColors",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Colors",
                schema: "inventory");
        }
    }
}
