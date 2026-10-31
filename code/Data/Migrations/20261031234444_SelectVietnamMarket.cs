using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SelectVietnamMarket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MarketId",
                table: "ShopSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ShopSettings" SET "MarketId" = (
                    SELECT "Id" FROM "Markets" WHERE "Name" = 'VIETNAM' ORDER BY "Id" LIMIT 1
                ) WHERE "Id" = 1 AND "MarketId" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSettings_MarketId",
                table: "ShopSettings",
                column: "MarketId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopSettings_Markets_MarketId",
                table: "ShopSettings",
                column: "MarketId",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopSettings_Markets_MarketId",
                table: "ShopSettings");

            migrationBuilder.DropIndex(
                name: "IX_ShopSettings_MarketId",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "ShopSettings");
        }
    }
}
