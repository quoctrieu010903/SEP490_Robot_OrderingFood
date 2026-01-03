using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderItemAndCreadQuickServeItem_010126 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complain_Tables_TableId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_Complain_Users_HandledBy",
                table: "Complain");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Complain",
                table: "Complain");

            migrationBuilder.RenameTable(
                name: "Complain",
                newName: "Complains");

            migrationBuilder.RenameIndex(
                name: "IX_Complain_TableId",
                table: "Complains",
                newName: "IX_Complains_TableId");

            migrationBuilder.RenameIndex(
                name: "IX_Complain_HandledBy",
                table: "Complains",
                newName: "IX_Complains_HandledBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledTime",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyTime",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemakedTime",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServedTime",
                table: "OrderItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Complains",
                table: "Complains",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "QuickServeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplainId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickServeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickServeItems_Complains_ComplainId",
                        column: x => x.ComplainId,
                        principalTable: "Complains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuickServeItems_ComplainId",
                table: "QuickServeItems",
                column: "ComplainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complains_Tables_TableId",
                table: "Complains",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complains_Users_HandledBy",
                table: "Complains",
                column: "HandledBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complains_Tables_TableId",
                table: "Complains");

            migrationBuilder.DropForeignKey(
                name: "FK_Complains_Users_HandledBy",
                table: "Complains");

            migrationBuilder.DropTable(
                name: "QuickServeItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Complains",
                table: "Complains");

            migrationBuilder.DropColumn(
                name: "CancelledTime",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ReadyTime",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "RemakedTime",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ServedTime",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Complains",
                newName: "Complain");

            migrationBuilder.RenameIndex(
                name: "IX_Complains_TableId",
                table: "Complain",
                newName: "IX_Complain_TableId");

            migrationBuilder.RenameIndex(
                name: "IX_Complains_HandledBy",
                table: "Complain",
                newName: "IX_Complain_HandledBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Complain",
                table: "Complain",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_Tables_TableId",
                table: "Complain",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_Users_HandledBy",
                table: "Complain",
                column: "HandledBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
