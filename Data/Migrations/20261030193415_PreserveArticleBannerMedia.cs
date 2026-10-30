using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreserveArticleBannerMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 8,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 508,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 512,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 513,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 514,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 1155,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 2092,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 5289,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7299,
                column: "BannerImage",
                value: "");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7931,
                column: "BannerImage",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 8,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_597.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 508,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_593.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 512,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_596.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 513,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_595.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 514,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_594.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 1155,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_634.png");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 2092,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_695.png");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 5289,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_1_5153_7145.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7299,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_1_5807_1584.jpg");

            migrationBuilder.UpdateData(
                table: "NewsArticles",
                keyColumn: "Id",
                keyValue: 7931,
                column: "BannerImage",
                value: "/images/pages/upload_image/news/news_1_6050_1872.jpg");
        }
    }
}
