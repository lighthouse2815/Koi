using Koi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

[Collection("Shop brand settings")]
public class VietnamMigrationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ForeignSeedCatalogIsRemovedWhileVietnamBranchesArePreserved()
    {
        using var client = AdminTestClient.Anonymous(factory);
        Assert.True((await client.GetAsync("/Home/Store")).IsSuccessStatusCode);
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var singapore = AdminTestClient.SeedGuid("Markets", 51);
        var vietnam = AdminTestClient.SeedGuid("Markets", 56);
        Assert.False(await database.Markets.AnyAsync(value => value.Id == singapore));
        Assert.True(await database.Markets.AnyAsync(value => value.Id == vietnam));
        var archived = await database.Database.SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"ArchivedVietnamStores\" WHERE \"BrandData\"->>'Name' = 'KOI Café'").SingleAsync();
        Assert.Equal(6, archived);
        Assert.False(await database.Stores.AnyAsync(value => value.MarketId == vietnam && value.Brand.Name == "KOI Café"));
        Assert.True(await database.Stores.AnyAsync(value => value.MarketId == vietnam && value.Brand.Name == "KOI Thé"));
        Assert.True(await database.Menus.AnyAsync(value => value.MarketId == vietnam));
    }
}
