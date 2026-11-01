using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi.Web.Data.Migrations;

public partial class RenameDrinkCategory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Drinks_DrinkCategories_CategoryId", table: "Drinks");
        migrationBuilder.DropPrimaryKey(name: "PK_DrinkCategories", table: "DrinkCategories");
        migrationBuilder.RenameTable(name: "DrinkCategories", newName: "Category");
        migrationBuilder.AddPrimaryKey(name: "PK_Category", table: "Category", column: "Id");
        migrationBuilder.AddForeignKey(name: "FK_Drinks_Category_CategoryId", table: "Drinks", column: "CategoryId",
            principalTable: "Category", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Drinks_Category_CategoryId", table: "Drinks");
        migrationBuilder.DropPrimaryKey(name: "PK_Category", table: "Category");
        migrationBuilder.RenameTable(name: "Category", newName: "DrinkCategories");
        migrationBuilder.AddPrimaryKey(name: "PK_DrinkCategories", table: "DrinkCategories", column: "Id");
        migrationBuilder.AddForeignKey(name: "FK_Drinks_DrinkCategories_CategoryId", table: "Drinks", column: "CategoryId",
            principalTable: "DrinkCategories", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }
}
