using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

public class OrderingApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task CheckoutRechecksPriceAndPersistsBranchAndOrderSnapshots()
    {
        using var client = AdminTestClient.Anonymous(factory);
        _ = await client.GetAsync("/Order/Index");
        Guid branchId, drinkId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var original = await db.Stores.AsNoTracking().FirstAsync();
            var category = new DrinkCategory { Name = "Danh mục " + Guid.NewGuid(), Image = "test", IsPublished = true };
            var branch = new StoreEntry { Name = "Chi nhánh đặt hàng " + Guid.NewGuid(), Address = "Địa chỉ kiểm thử", MarketId = original.MarketId, BrandId = original.BrandId, AcceptsOrders = true, IsPublished = true };
            var drink = new Drink { Name = "Trà thử nghiệm " + Guid.NewGuid(), Category = category, Price = 40000, IsPublished = true };
            db.AddRange(branch, drink); await db.SaveChangesAsync(); branchId = branch.Id; drinkId = drink.Id;
        }
        Assert.Equal(HttpStatusCode.Redirect, (await AdminTestClient.Post(client, "/Order/SelectBranch", new() { ["storeId"] = branchId.ToString() }, tokenUrl: "/Admin/Account/Login")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await AdminTestClient.Post(client, "/Order/Add", new() { ["id"] = drinkId.ToString(), ["quantity"] = "2" }, tokenUrl: "/Admin/Account/Login")).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var drink = await db.Drinks.FindAsync(drinkId); drink!.Price = 45000; await db.SaveChangesAsync();
        }
        Dictionary<string, string> Fields() => new() { ["Checkout.CustomerName"] = "Người kiểm thử", ["Checkout.CustomerPhone"] = "0901234567", ["Checkout.Fulfillment"] = "0" };
        var changed = await AdminTestClient.Post(client, "/Order/Checkout", Fields(), tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Contains("Giá đã thay đổi", WebUtility.HtmlDecode(await changed.Content.ReadAsStringAsync()));
        var checkout = await AdminTestClient.Post(client, "/Order/Checkout", Fields(), tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, checkout.StatusCode);
        Assert.Contains("/Order/Complete", checkout.Headers.Location!.ToString());
        using var verify = factory.Services.CreateScope();
        var database = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await database.Orders.Include(value => value.Items).SingleAsync(value => value.StoreId == branchId);
        Assert.Equal("Địa chỉ kiểm thử", order.StoreAddress);
        Assert.Equal("Người kiểm thử", order.CustomerName);
        Assert.Equal(45000m, Assert.Single(order.Items).UnitPrice);
        Assert.Equal(2, order.Items.Single().Quantity);
        Assert.Equal(OrderStatus.New, order.Status);
    }

    [Fact]
    public async Task RejectsClosedBranchesAndRequiresAddressForDelivery()
    {
        using var client = AdminTestClient.Anonymous(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await AdminTestClient.Post(client, "/Order/SelectBranch", new() { ["storeId"] = Guid.NewGuid().ToString() }, tokenUrl: "/Admin/Account/Login")).StatusCode);
        var response = await AdminTestClient.Post(client, "/Order/Checkout", new() { ["Checkout.CustomerName"] = "Test", ["Checkout.CustomerPhone"] = "0901234567", ["Checkout.Fulfillment"] = "1" }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Nhập địa chỉ giao hàng", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }
}
