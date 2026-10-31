using System.Text.Json;
using Koi.Web.Models;

namespace Koi.Web.Data;

public static class StoreCatalog
{
    public static StoreSnapshot Snapshot { get; } = Load();

    private static StoreSnapshot Load()
    {
        using var stream = typeof(StoreCatalog).Assembly.GetManifestResourceStream("Koi.Web.Data.Stores.json")
            ?? throw new InvalidOperationException("Store catalog is missing.");
        return JsonSerializer.Deserialize<StoreSnapshot>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Store catalog is empty.");
    }
}

// Shapes used to read the original catalog for database migrations.
public record StoreSnapshot(string CapturedOn, SeedCountry[] Countries, SeedLocation[] Locations,
    StoreBrand[] Brands, SeedBranch[] Stores);
public record SeedCountry(int Id, string Name, int[] BrandIds, StoreSocialLink[] SocialLinks, int[] StoreIds);
public record SeedLocation(int Id, int CountryId, string Name, int[] StoreIds);
public record SeedBranch(int Id, string Name, string Phone, string Hours, string Address,
    int CountryId, int LocationId, int BrandId, string Logo, string Photo, string NavigationUrl);
