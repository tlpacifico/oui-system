using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oui.Modules.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFirebaseUidToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirebaseUid",
                schema: "auth",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirebaseUid",
                schema: "auth",
                table: "Users",
                column: "FirebaseUid",
                unique: true,
                filter: "\"FirebaseUid\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_FirebaseUid",
                schema: "auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirebaseUid",
                schema: "auth",
                table: "Users");
        }
    }
}
