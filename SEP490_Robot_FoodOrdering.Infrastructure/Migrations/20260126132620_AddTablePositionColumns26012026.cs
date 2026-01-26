using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePositionColumns26012026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionX",
                table: "Tables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionY",
                table: "Tables",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "Tables");
        }
    }
}
