using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEP490_Robot_FoodOrdering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateFeedbackAndComplainDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complain_OrderItems_OrderItemId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_OrderItems_OrderItemId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_OrderItemId",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Complain_OrderItemId",
                table: "Complain");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Complain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemId",
                table: "Feedback",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemId",
                table: "Complain",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_OrderItemId",
                table: "Feedback",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Complain_OrderItemId",
                table: "Complain",
                column: "OrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_OrderItems_OrderItemId",
                table: "Complain",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_OrderItems_OrderItemId",
                table: "Feedback",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id");
        }
    }
}
