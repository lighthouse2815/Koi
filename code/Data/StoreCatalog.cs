using System.Text.Json;

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

// Shapes used to load the initial Vietnamese branch catalog.
public record StoreSnapshot(SeedLocation[] Locations, SeedBranch[] Stores);
public record SeedLocation(int Id, string Name);
public record SeedBranch(int Id, string Name, string Phone, string Hours, string Address,
    int LocationId, string Photo, string NavigationUrl);
