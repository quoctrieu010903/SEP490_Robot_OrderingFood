using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIsServedQuickServeItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsServed",
                table: "QuickServeItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsServed",
                table: "QuickServeItems");
        }
    }
}
