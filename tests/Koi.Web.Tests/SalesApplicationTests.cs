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
            var original = await db.Stores.AsNoTracking().FirstAsync();
            var drink = new Drink { Name = "Giá kiểm thử " + Guid.NewGuid(), CategoryId = category.Id };
            var branch = new StoreEntry { Name = "Chi nhánh kiểm thử " + Guid.NewGuid(), MarketId = original.MarketId, BrandId = original.BrandId };
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
    public async Task ChoosingABrandLimitsBranchesThatCanReceiveOrders()
    {
        using var admin = await AdminTestClient.Login(factory);
        using var customer = AdminTestClient.Anonymous(factory);
        Guid brandId, otherBrandId, firstId, secondId;
        Guid? previous;
        string firstName = "Brand branch " + Guid.NewGuid(), secondName = "Other branch " + Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            previous = (await db.ShopSettings.FindAsync(1))!.BrandId;
            var brands = await db.TeaBrands.OrderBy(value => value.Id).Take(2).ToArrayAsync();
            brandId = brands[0].Id; otherBrandId = brands[1].Id;
            var marketId = (await db.Markets.FirstAsync()).Id;
            var first = new StoreEntry { Name = firstName, BrandId = brandId, MarketId = marketId, AcceptsOrders = true };
            var second = new StoreEntry { Name = secondName, BrandId = otherBrandId, MarketId = marketId, AcceptsOrders = true };
            db.AddRange(first, second); await db.SaveChangesAsync(); firstId = first.Id; secondId = second.Id;
        }
        try
        {
            var select = await AdminTestClient.Post(admin, "/Admin/Sales/Brand", new() { ["brandId"] = brandId.ToString() }, tokenUrl: "/Admin/Sales");
            Assert.Equal(HttpStatusCode.Redirect, select.StatusCode);
            var page = WebUtility.HtmlDecode(await customer.GetStringAsync("/Order/Index"));
            Assert.Contains(firstName, page);
            Assert.DoesNotContain(secondName, page);
            var denied = await AdminTestClient.Post(customer, "/Order/SelectBranch", new() { ["storeId"] = secondId.ToString() }, tokenUrl: "/Admin/Account/Login");
            Assert.Equal(HttpStatusCode.BadRequest, denied.StatusCode);
            var allowed = await AdminTestClient.Post(customer, "/Order/SelectBranch", new() { ["storeId"] = firstId.ToString() }, tokenUrl: "/Admin/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, allowed.StatusCode);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var settings = await db.ShopSettings.FindAsync(1); settings!.BrandId = previous;
            var branches = await db.Stores.Where(value => value.Id == firstId || value.Id == secondId).ToArrayAsync();
            foreach (var branch in branches) branch.AcceptsOrders = false;
            await db.SaveChangesAsync();
        }
    }

}
