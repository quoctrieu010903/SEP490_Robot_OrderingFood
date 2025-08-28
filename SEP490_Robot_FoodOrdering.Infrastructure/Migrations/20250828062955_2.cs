using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Invoices_InvoiceId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_InvoiceId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "OrderItems");

            migrationBuilder.CreateTable(
                name: "InvoiceDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    totalMoney = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDetail_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDetail_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_InvoiceId",
                table: "InvoiceDetail",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_OrderItemId",
                table: "InvoiceDetail",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceDetail");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_InvoiceId",
                table: "OrderItems",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Invoices_InvoiceId",
                table: "OrderItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }
    }
}
