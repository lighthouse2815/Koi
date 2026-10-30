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
