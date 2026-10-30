using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data;

internal static class VietnameseSeed
{
    public static void Insert(MigrationBuilder migration)
    {
        using var stream = typeof(VietnameseSeed).Assembly.GetManifestResourceStream("Koi.Web.Data.VietnameseSeed.json")
            ?? throw new InvalidOperationException("Vietnamese seed data is missing.");
        using var document = JsonDocument.Parse(stream);
        foreach (var (group, table, fields) in new (string, string, string[])[]
        {
            ("news", "NewsArticles", ["TitleVi", "BodyHtmlVi"]),
            ("categories", "DrinkCategories", ["NameVi"]),
            ("drinks", "Drinks", ["NameVi", "RegionVi", "DescriptionVi"]),
            ("featured", "FeaturedDrinks", ["NameVi", "RegionVi"]),
            ("markets", "Markets", ["NameVi"]),
            ("cities", "StoreCities", ["NameVi"])
        })
        {
            foreach (var row in document.RootElement.GetProperty(group).EnumerateArray())
                migration.UpdateData(table, "Id", row.GetProperty("id").GetInt32(), fields,
                    fields.Select(field => (object)(row.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(field)).GetString() ?? "")).ToArray());
        }
    }
}
