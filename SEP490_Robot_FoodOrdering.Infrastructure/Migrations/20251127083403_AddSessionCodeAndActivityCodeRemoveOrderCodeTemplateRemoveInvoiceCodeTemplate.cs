using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionCodeAndActivityCodeRemoveOrderCodeTemplateRemoveInvoiceCodeTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionCode",
                table: "TableSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivityCode",
                table: "TableActivitys",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionCode",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "ActivityCode",
                table: "TableActivitys");
        }
    }
}
