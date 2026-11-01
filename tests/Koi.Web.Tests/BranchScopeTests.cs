using System.Net;
using Koi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

[Collection("Shop brand settings")]
public class BranchScopeTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task OtherMarketsCannotBeManagedThroughDirectUrls()
    {
        using var admin = await AdminTestClient.Login(factory);
        Guid foreignStoreId, foreignMenuId, foreignMarketId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var settings = await db.ShopSettings.SingleAsync();
            var store = await db.Stores.AsNoTracking().FirstAsync(value => value.MarketId != settings.MarketId);
            var menu = await db.Menus.AsNoTracking().FirstAsync(value => value.MarketId != settings.MarketId);
            foreignStoreId = store.Id; foreignMenuId = menu.Id; foreignMarketId = menu.MarketId;
        }
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Stores/Edit/{foreignStoreId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Stores/Delete/{foreignStoreId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Menus/Edit/{foreignMenuId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Menus/Delete/{foreignMenuId}")).StatusCode);
        var result = await AdminTestClient.Post(admin, "/Admin/Menus/Create", new() { ["Label"] = "Invalid foreign menu", ["MarketId"] = foreignMarketId.ToString() }, AdminTestClient.Image, "/Admin/Menus/Create");
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        using var verify = factory.Services.CreateScope();
        var database = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await database.Stores.AnyAsync(value => value.Id == foreignStoreId));
        Assert.True(await database.Menus.AnyAsync(value => value.Id == foreignMenuId));
    }
}
