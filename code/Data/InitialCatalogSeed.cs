using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data;

internal static class InitialCatalogSeed
{
    private record Catalog(SeedCategory[] Categories, SeedFeaturedDrink[] Featured);
    private record SeedCategory(int Id, string Name, string Icon, string Image, bool ShowOnHome, int SortOrder, SeedDrink[] Drinks);
    private record SeedDrink(int Id, string Name, string Description, string Image, int SortOrder, decimal Price);
    private record SeedFeaturedDrink(int Id, string Name, string Image, string BackgroundImage, int SortOrder);

    public static void Insert(MigrationBuilder migration)
    {
        using var stream = typeof(InitialCatalogSeed).Assembly.GetManifestResourceStream("Koi.Web.Data.DrinksSeed.json")
            ?? throw new InvalidOperationException("Drink seed data is missing.");
        var catalog = JsonSerializer.Deserialize<Catalog>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Drink seed data is empty.");
        foreach (var category in catalog.Categories)
        {
            var categoryId = Id("DrinkCategories", category.Id);
            migration.InsertData("Category", ["Id", "Name", "Icon", "Image", "ShowOnHome", "IsPublished", "SortOrder", "Revision"],
                [categoryId, category.Name, category.Icon, category.Image, category.ShowOnHome, true, category.SortOrder, 1]);
            foreach (var drink in category.Drinks)
                migration.InsertData("Drinks", ["Id", "CategoryId", "Name", "Description", "Image", "Price", "SortOrder", "IsPublished", "Revision"],
                    [Id("Drinks", drink.Id), categoryId, drink.Name, drink.Description, drink.Image, drink.Price, drink.SortOrder, true, 1]);
        }
        foreach (var drink in catalog.Featured)
            migration.InsertData("FeaturedDrinks", ["Id", "Name", "Image", "BackgroundImage", "SortOrder", "IsPublished", "Revision"],
                [Id("FeaturedDrinks", drink.Id), drink.Name, drink.Image, drink.BackgroundImage, drink.SortOrder, true, 1]);

        var snapshot = StoreCatalog.Snapshot;
        foreach (var (city, order) in snapshot.Locations.Select((value, index) => (value, index)))
            migration.InsertData("StoreCities", ["Id", "Name", "SortOrder", "Revision"], [Id("StoreCities", city.Id), city.Name, order, 1]);
        foreach (var store in snapshot.Stores)
            migration.InsertData("Stores", ["Id", "Name", "Phone", "Hours", "Address", "CityId", "Photo", "NavigationUrl", "AcceptsOrders", "IsPublished", "SortOrder", "Revision"],
                [Id("Stores", store.Id), store.Name, store.Phone, store.Hours, store.Address, store.LocationId == 0 ? null! : Id("StoreCities", store.LocationId), store.Photo, store.NavigationUrl, true, true, store.Id, 1]);

        var menus = MenuCatalog.Documents;
        for (var order = 0; order < menus.Count; order++)
            migration.InsertData("Menus", ["Id", "Label", "File", "SortOrder", "IsPublished", "Revision"],
                [Id("Menus", order + 1), menus[order].Label, "/menus/" + menus[order].File, order, true, 1]);
    }

    private static Guid Id(string table, int value)
    {
        var prefix = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(table)))[..24];
        return Guid.ParseExact($"{prefix}{value:x8}", "N");
    }
}
