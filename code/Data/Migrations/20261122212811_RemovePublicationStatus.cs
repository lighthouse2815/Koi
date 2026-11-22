using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePublicationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_IsPublished_CityId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_IsPublished_Date",
                table: "NewsArticles");

            migrationBuilder.DropIndex(
                name: "IX_Menus_IsPublished_SortOrder",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Drinks_CategoryId_IsPublished_SortOrder",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "FeaturedDrinks");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Date",
                table: "NewsArticles",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_SortOrder",
                table: "Menus",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_CategoryId_SortOrder",
                table: "Drinks",
                columns: new[] { "CategoryId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsArticles_Date",
                table: "NewsArticles");

            migrationBuilder.DropIndex(
                name: "IX_Menus_SortOrder",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Drinks_CategoryId_SortOrder",
                table: "Drinks");

            // The original publication flags are unavailable. Preserve current visibility on rollback.
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Stores",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "NewsArticles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Menus",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "FeaturedDrinks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Drinks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Category",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsPublished_CityId",
                table: "Stores",
                columns: new[] { "IsPublished", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_IsPublished_Date",
                table: "NewsArticles",
                columns: new[] { "IsPublished", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IsPublished_SortOrder",
                table: "Menus",
                columns: new[] { "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_CategoryId_IsPublished_SortOrder",
                table: "Drinks",
                columns: new[] { "CategoryId", "IsPublished", "SortOrder" });
        }
    }
}
