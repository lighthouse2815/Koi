using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeaBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeaBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSocialLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSocialLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketSocialLink_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreCities_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketBrands",
                columns: table => new
                {
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketBrands", x => new { x.MarketId, x.BrandId });
                    table.ForeignKey(
                        name: "FK_MarketBrands_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketBrands_TeaBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "TeaBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Hours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: true),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NavigationUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CountryOrder = table.Column<int>(type: "integer", nullable: false),
                    CityOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_StoreCities_CityId",
                        column: x => x.CityId,
                        principalTable: "StoreCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_TeaBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "TeaBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketBrands_BrandId",
                table: "MarketBrands",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSocialLink_MarketId",
                table: "MarketSocialLink",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCities_MarketId",
                table: "StoreCities",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_BrandId",
                table: "Stores",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsPublished_MarketId_CityId_BrandId",
                table: "Stores",
                columns: new[] { "IsPublished", "MarketId", "CityId", "BrandId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_MarketId",
                table: "Stores",
                column: "MarketId");
            StoreSeed.Insert(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketBrands");

            migrationBuilder.DropTable(
                name: "MarketSocialLink");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "StoreCities");

            migrationBuilder.DropTable(
                name: "TeaBrands");

            migrationBuilder.DropTable(
                name: "Markets");
        }
    }
}
