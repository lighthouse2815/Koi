using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations;

public partial class RetainUnusedSaleOptions : Migration
{
    // The application no longer uses SaleOptions; keep the existing table and its data.
    protected override void Up(MigrationBuilder migrationBuilder) { }

    protected override void Down(MigrationBuilder migrationBuilder) { }
}
