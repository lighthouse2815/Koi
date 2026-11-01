using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
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
            var foreignMarket = new Market { Name = "Thị trường kiểm thử " + Guid.NewGuid() };
            var store = new StoreEntry { Name = "Chi nhánh kiểm thử", Market = foreignMarket, BrandId = settings.BrandId!.Value };
            var menu = new MenuEntry { Label = "Thực đơn kiểm thử", File = "/test.png", Market = foreignMarket };
            db.AddRange(store, menu);
            await db.SaveChangesAsync();
            foreignStoreId = store.Id; foreignMenuId = menu.Id; foreignMarketId = foreignMarket.Id;
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
