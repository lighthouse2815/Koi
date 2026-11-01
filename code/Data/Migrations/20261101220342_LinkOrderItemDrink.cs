using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkOrderItemDrink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DrinkId",
                table: "OrderItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_DrinkId",
                table: "OrderItem",
                column: "DrinkId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Drinks_DrinkId",
                table: "OrderItem",
                column: "DrinkId",
                principalTable: "Drinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Drinks_DrinkId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_DrinkId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "DrinkId",
                table: "OrderItem");
        }
    }
}
