using System.Text.Json;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data;

internal static class DrinkSeed
{
    private record Catalog(DrinkCategory[] Categories, FeaturedDrink[] Featured);
    public static void Insert(MigrationBuilder migration)
    {
        using var stream = typeof(DrinkSeed).Assembly.GetManifestResourceStream("Koi.Web.Data.DrinksSeed.json")
            ?? throw new InvalidOperationException("Drink seed data is missing.");
        var data = JsonSerializer.Deserialize<Catalog>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        foreach (var category in data.Categories)
        {
            migration.InsertData("DrinkCategories", ["Id", "Name", "Icon", "Image", "HomeImage", "HomeIcon", "ShowOnHome", "IsPublished", "SortOrder", "HomeOrder", "Revision"],
                [category.Id, category.Name, category.Icon, category.Image, category.HomeImage, category.HomeIcon, category.ShowOnHome, true, category.SortOrder, category.HomeOrder, 1]);
            foreach (var drink in category.Drinks)
                migration.InsertData("Drinks", ["Id", "CategoryId", "Name", "Region", "Description", "Image", "SortOrder", "IsPublished", "Revision"],
                    [drink.Id, category.Id, drink.Name, drink.Region, drink.Description, drink.Image, drink.SortOrder, true, 1]);
        }
        foreach (var feature in data.Featured)
            migration.InsertData("FeaturedDrinks", ["Id", "Name", "Region", "Image", "BackgroundImage", "SortOrder", "IsPublished", "Revision"],
                [feature.Id, feature.Name, feature.Region, feature.Image, feature.BackgroundImage, feature.SortOrder, true, 1]);
    }
}
