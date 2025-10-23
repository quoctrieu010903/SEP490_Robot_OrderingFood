using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentPolicy",
                table: "SystemSettings",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "SystemSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "SystemSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "SystemSettings");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "SystemSettings",
                newName: "PaymentPolicy");
        }
    }
}
