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
        var obsoleteTables = await database.Database.SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM pg_tables WHERE schemaname = 'public' AND tablename IN ('Markets', 'TeaBrands', 'MarketBrands')").SingleAsync();
        Assert.Equal(0, obsoleteTables);
        var archived = await database.Database.SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"ArchivedVietnamStores\" WHERE \"BrandData\"->>'Name' = 'KOI Café'").SingleAsync();
        Assert.Equal(6, archived);
        Assert.Equal("KOI Thé", (await database.ShopSettings.SingleAsync()).Name);
        Assert.True(await database.Stores.CountAsync() >= 67);
        Assert.True(await database.Menus.AnyAsync());
        Assert.Equal(2, await database.SocialLinks.CountAsync());
    }
}
