using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVietnameseContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "TeaBrands",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressVi",
                table: "Stores",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HoursVi",
                table: "Stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "Stores",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "StoreCities",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BodyHtmlVi",
                table: "NewsArticles",
                type: "character varying(200000)",
                maxLength: 200000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleVi",
                table: "NewsArticles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LabelVi",
                table: "Menus",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "Markets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "FeaturedDrinks",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegionVi",
                table: "FeaturedDrinks",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionVi",
                table: "Drinks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "Drinks",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegionVi",
                table: "Drinks",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameVi",
                table: "DrinkCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 508,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 512,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 513,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 514,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 1155,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 2092,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 2782,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 5253,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 5289,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7299,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7931,
                columns: new[] { "BodyHtmlVi", "TitleVi" },
                values: new object[] { "", "" });
            VietnameseSeed.Insert(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "TeaBrands");

            migrationBuilder.DropColumn(
                name: "AddressVi",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "HoursVi",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "StoreCities");

            migrationBuilder.DropColumn(
                name: "BodyHtmlVi",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "TitleVi",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "LabelVi",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "FeaturedDrinks");

            migrationBuilder.DropColumn(
                name: "RegionVi",
                table: "FeaturedDrinks");

            migrationBuilder.DropColumn(
                name: "DescriptionVi",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "RegionVi",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "NameVi",
                table: "DrinkCategories");
        }
    }
}
