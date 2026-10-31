using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

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
}
