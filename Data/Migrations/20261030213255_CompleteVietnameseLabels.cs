using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteVietnameseLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("Drinks", "Id", 6, "RegionVi", "Việt Nam");
            migrationBuilder.UpdateData("Menus", "Id", 4, "LabelVi", "KOI PLUS - Trung Quốc");
            migrationBuilder.UpdateData("Menus", "Id", 5, "LabelVi", "KOI Thé - Ma Cao, Trung Quốc");
            migrationBuilder.UpdateData("Menus", "Id", 6, "LabelVi", "KOI Thé - Hồng Kông, Trung Quốc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
