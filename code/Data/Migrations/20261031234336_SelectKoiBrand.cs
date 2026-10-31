using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SelectKoiBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "ShopSettings" SET "BrandId" = (
                    SELECT "Id" FROM "TeaBrands" WHERE "Name" = 'KOI Thé' ORDER BY "Id" LIMIT 1
                ) WHERE "Id" = 1 AND "BrandId" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep the configured brand when rolling back; it may have been changed by an administrator.
        }
    }
}
