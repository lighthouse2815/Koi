using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyVietnamBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "ArchivedVietnamMetadata" ("Kind" text NOT NULL, "Data" jsonb NOT NULL);
                INSERT INTO "ArchivedVietnamMetadata" ("Kind", "Data") SELECT 'Market', to_jsonb(value) FROM "Markets" value;
                INSERT INTO "ArchivedVietnamMetadata" ("Kind", "Data") SELECT 'Brand', to_jsonb(value) FROM "TeaBrands" value;
                INSERT INTO "ArchivedVietnamMetadata" ("Kind", "Data") SELECT 'MarketBrand', to_jsonb(value) FROM "MarketBrands" value;
                ALTER TABLE "ShopSettings" ADD COLUMN "Name" varchar(150) NOT NULL DEFAULT '';
                ALTER TABLE "ShopSettings" ADD COLUMN "NameVi" varchar(150) NOT NULL DEFAULT '';
                ALTER TABLE "ShopSettings" ADD COLUMN "Logo" varchar(500) NOT NULL DEFAULT '';
                UPDATE "ShopSettings" settings SET "Name" = brand."Name", "NameVi" = brand."NameVi", "Logo" = brand."Logo"
                    FROM "TeaBrands" brand WHERE settings."BrandId" = brand."Id";
                CREATE TABLE "SocialLinks" ("Id" uuid NOT NULL PRIMARY KEY, "Label" varchar(100) NOT NULL, "Url" varchar(1000) NOT NULL, "SortOrder" integer NOT NULL);
                INSERT INTO "SocialLinks" ("Id", "Label", "Url", "SortOrder")
                    SELECT link."Id", link."Label", link."Url", link."SortOrder" FROM "MarketSocialLink" link
                    JOIN "ShopSettings" settings ON settings."MarketId" = link."MarketId" WHERE settings."Id" = 1;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Markets_MarketId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopSettings_Markets_MarketId",
                table: "ShopSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopSettings_TeaBrands_BrandId",
                table: "ShopSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreCities_Markets_MarketId",
                table: "StoreCities");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Markets_MarketId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_TeaBrands_BrandId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "MarketBrands");

            migrationBuilder.DropTable(
                name: "MarketSocialLink");

            migrationBuilder.DropTable(
                name: "TeaBrands");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropIndex(
                name: "IX_Stores_BrandId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_IsPublished_MarketId_CityId_BrandId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_MarketId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_StoreCities_MarketId",
                table: "StoreCities");

            migrationBuilder.DropIndex(
                name: "IX_ShopSettings_BrandId",
                table: "ShopSettings");

            migrationBuilder.DropIndex(
                name: "IX_ShopSettings_MarketId",
                table: "ShopSettings");

            migrationBuilder.DropIndex(
                name: "IX_Menus_MarketId_IsPublished_SortOrder",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "StoreCities");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "Menus");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsPublished_CityId",
                table: "Stores",
                columns: new[] { "IsPublished", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IsPublished_SortOrder",
                table: "Menus",
                columns: new[] { "IsPublished", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Restore the archived Vietnam metadata explicitly before rolling back this migration.");
        }
    }
}
