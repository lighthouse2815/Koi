using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261102210000_EnableSeedOrdering")]
public class EnableSeedOrdering : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        using var stream = typeof(EnableSeedOrdering).Assembly.GetManifestResourceStream("Koi.Web.Data.DrinksSeed.json")
            ?? throw new InvalidOperationException("Drink seed data is missing.");
        using var catalog = JsonDocument.Parse(stream);
        foreach (var category in catalog.RootElement.GetProperty("categories").EnumerateArray())
        foreach (var drink in category.GetProperty("drinks").EnumerateArray())
        {
            var id = SeedId("Drinks", drink.GetProperty("id").GetInt32());
            var price = drink.GetProperty("price").GetDecimal();
            migrationBuilder.Sql($"UPDATE \"Drinks\" SET \"Price\" = {price.ToString(System.Globalization.CultureInfo.InvariantCulture)} WHERE \"Id\" = '{id}' AND \"Price\" IS NULL;");
        }

        foreach (var store in StoreCatalog.Snapshot.Stores)
        {
            var id = SeedId("Stores", store.Id);
            migrationBuilder.Sql($"UPDATE \"Stores\" SET \"AcceptsOrders\" = TRUE WHERE \"Id\" = '{id}';");
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Restore a backup to undo the seed ordering update.");

    private static Guid SeedId(string table, int value)
    {
        var prefix = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(table)))[..24];
        return Guid.ParseExact($"{prefix}{value:x8}", "N");
    }
}
