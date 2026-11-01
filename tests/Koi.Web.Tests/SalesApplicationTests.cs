using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

[CollectionDefinition("Shop brand settings", DisableParallelization = true)]
public class ShopBrandSettingsCollection;

[Collection("Shop brand settings")]
public class SalesApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task AdminCanSetPriceAndEnableABranchWithoutExposingSettings()
    {
        using var anonymous = AdminTestClient.Anonymous(factory);
        Assert.Equal(HttpStatusCode.Redirect, (await anonymous.GetAsync("/Admin/Sales")).StatusCode);
        using var admin = await AdminTestClient.Login(factory);
        Guid drinkId, branchId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = await db.DrinkCategories.FirstAsync();
            var drink = new Drink { Name = "Giá kiểm thử " + Guid.NewGuid(), CategoryId = category.Id };
            var branch = new StoreEntry { Name = "Chi nhánh kiểm thử " + Guid.NewGuid() };
            db.AddRange(drink, branch);
            await db.SaveChangesAsync();
            drinkId = drink.Id; branchId = branch.Id;
        }
        var price = await AdminTestClient.Post(admin, "/Admin/Sales/Price", new() { ["id"] = drinkId.ToString(), ["revision"] = "1", ["price"] = "45000" }, tokenUrl: "/Admin/Sales");
        Assert.Equal(HttpStatusCode.Redirect, price.StatusCode);
        var branchResult = await AdminTestClient.Post(admin, "/Admin/Sales/Branch", new() { ["id"] = branchId.ToString(), ["revision"] = "1", ["acceptsOrders"] = "true" }, tokenUrl: "/Admin/Sales");
        Assert.Equal(HttpStatusCode.Redirect, branchResult.StatusCode);
        using var verify = factory.Services.CreateScope();
        var database = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(45000m, (await database.Drinks.FindAsync(drinkId))!.Price);
        Assert.True((await database.Stores.FindAsync(branchId))!.AcceptsOrders);
        var stale = await AdminTestClient.Post(admin, "/Admin/Sales/Price", new() { ["id"] = drinkId.ToString(), ["revision"] = "1", ["price"] = "1" }, tokenUrl: "/Admin/Sales");
        Assert.Equal(HttpStatusCode.Redirect, stale.StatusCode);
        await database.Entry((await database.Drinks.FindAsync(drinkId))!).ReloadAsync();
        Assert.Equal(45000m, (await database.Drinks.FindAsync(drinkId))!.Price);
    }
    [Fact]
    public async Task OnlyEnabledBranchesCanReceiveOrders()
    {
        using var customer = AdminTestClient.Anonymous(factory);
        Guid enabledId, disabledId;
        string enabledName = "Chi nhánh mở " + Guid.NewGuid(), disabledName = "Chi nhánh đóng " + Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enabled = new StoreEntry { Name = enabledName, AcceptsOrders = true };
            var disabled = new StoreEntry { Name = disabledName, AcceptsOrders = false };
            db.AddRange(enabled, disabled); await db.SaveChangesAsync(); enabledId = enabled.Id; disabledId = disabled.Id;
        }
        var page = WebUtility.HtmlDecode(await customer.GetStringAsync("/Order/Index"));
        Assert.Contains(enabledName, page);
        Assert.DoesNotContain(disabledName, page);
        var denied = await AdminTestClient.Post(customer, "/Order/SelectBranch", new() { ["storeId"] = disabledId.ToString() }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.BadRequest, denied.StatusCode);
        var allowed = await AdminTestClient.Post(customer, "/Order/SelectBranch", new() { ["storeId"] = enabledId.ToString() }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, allowed.StatusCode);
    }

}
