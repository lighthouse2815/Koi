using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveOtherVietnamBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE "ArchivedVietnamStores" (
                    "Id" uuid PRIMARY KEY,
                    "StoreData" jsonb NOT NULL,
                    "BrandData" jsonb NOT NULL
                );
                INSERT INTO "ArchivedVietnamStores" ("Id", "StoreData", "BrandData")
                SELECT store."Id", to_jsonb(store), to_jsonb(brand)
                FROM "Stores" store
                JOIN "TeaBrands" brand ON brand."Id" = store."BrandId"
                JOIN "ShopSettings" settings ON settings."Id" = 1 AND store."MarketId" = settings."MarketId"
                WHERE settings."BrandId" IS NOT NULL AND store."BrandId" <> settings."BrandId";
                DELETE FROM "Stores" WHERE "Id" IN (SELECT "Id" FROM "ArchivedVietnamStores");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Archived Vietnamese stores require an explicit data restore.");
        }
    }
}
