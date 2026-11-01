using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations;

public partial class RetainUnusedSocialLinks : Migration
{
    // Keep the former social links table and its data outside the application model.
    protected override void Up(MigrationBuilder migrationBuilder) { }

    protected override void Down(MigrationBuilder migrationBuilder) { }
}
