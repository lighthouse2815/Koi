using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveForeignCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE vietnam_id uuid;
                BEGIN
                    SELECT m."Id" INTO vietnam_id
                    FROM "ShopSettings" s JOIN "Markets" m ON m."Id" = s."MarketId"
                    WHERE s."Id" = 1 AND m."Name" = 'VIETNAM';
                    IF vietnam_id IS NULL THEN
                        RAISE EXCEPTION 'Shop must be configured for Vietnam before removing foreign catalog data';
                    END IF;
                    DELETE FROM "Menus" WHERE "MarketId" <> vietnam_id;
                    DELETE FROM "Stores" WHERE "MarketId" <> vietnam_id;
                    DELETE FROM "StoreCities" WHERE "MarketId" <> vietnam_id;
                    DELETE FROM "Markets" WHERE "Id" <> vietnam_id;
                    DELETE FROM "TeaBrands" b
                    WHERE NOT EXISTS (SELECT 1 FROM "Stores" s WHERE s."BrandId" = b."Id")
                      AND NOT EXISTS (SELECT 1 FROM "MarketBrands" mb WHERE mb."BrandId" = b."Id")
                      AND NOT EXISTS (SELECT 1 FROM "ShopSettings" cfg WHERE cfg."BrandId" = b."Id");
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Foreign catalog data was permanently removed and cannot be restored by rolling back the migration.");
        }
    }
}
