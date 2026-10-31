using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

[Collection("Shop brand settings")]
public class OrderingApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task CheckoutRechecksPriceAndPersistsBranchAndOrderSnapshots()
    {
        await using var brand = await SalesTestBrand.SelectFirst(factory);
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
        await using var brand = await SalesTestBrand.SelectFirst(factory);
        using var client = AdminTestClient.Anonymous(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await AdminTestClient.Post(client, "/Order/SelectBranch", new() { ["storeId"] = Guid.NewGuid().ToString() }, tokenUrl: "/Admin/Account/Login")).StatusCode);
        var response = await AdminTestClient.Post(client, "/Order/Checkout", new() { ["Checkout.CustomerName"] = "Test", ["Checkout.CustomerPhone"] = "0901234567", ["Checkout.Fulfillment"] = "1" }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Nhập địa chỉ giao hàng", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }
    [Fact]
    public async Task CustomizationsArePricedAndSavedForEachCup()
    {
        await using var brand = await SalesTestBrand.SelectFirst(factory);
        using var client = AdminTestClient.Anonymous(factory);
        Guid branchId, drinkId, sizeId, toppingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var source = await db.Stores.AsNoTracking().FirstAsync();
            var category = new DrinkCategory { Name = "Danh mục lựa chọn " + Guid.NewGuid(), Image = "test" };
            var drink = new Drink { Name = "Trà lựa chọn " + Guid.NewGuid(), Category = category, Price = 30000 };
            var branch = new StoreEntry { Name = "Chi nhánh lựa chọn " + Guid.NewGuid(), Address = "Địa chỉ", MarketId = source.MarketId, BrandId = source.BrandId, AcceptsOrders = true };
            var size = new SaleOption { Name = "Lớn", Kind = SaleOptionKind.Size, ExtraPrice = 5000 };
            var topping = new SaleOption { Name = "Trân châu", Kind = SaleOptionKind.Topping, ExtraPrice = 10000 };
            db.AddRange(drink, branch, size, topping); await db.SaveChangesAsync();
            branchId = branch.Id; drinkId = drink.Id; sizeId = size.Id; toppingId = topping.Id;
        }
        try
        {
            await AdminTestClient.Post(client, "/Order/SelectBranch", new() { ["storeId"] = branchId.ToString() }, tokenUrl: "/Admin/Account/Login");
            var add = await AdminTestClient.Post(client, "/Order/Add", new() { ["id"] = drinkId.ToString(), ["quantity"] = "2", ["sizeId"] = sizeId.ToString(), ["toppingIds"] = toppingId.ToString(), ["sugar"] = "1", ["ice"] = "2" }, tokenUrl: "/Admin/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, add.StatusCode);
            var second = await AdminTestClient.Post(client, "/Order/Add", new() { ["id"] = drinkId.ToString(), ["quantity"] = "1", ["sizeId"] = sizeId.ToString() }, tokenUrl: "/Admin/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, second.StatusCode);
            var checkout = await AdminTestClient.Post(client, "/Order/Checkout", new() { ["Checkout.CustomerName"] = "Khách chọn món", ["Checkout.CustomerPhone"] = "0901234567", ["Checkout.Fulfillment"] = "0" }, tokenUrl: "/Admin/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, checkout.StatusCode);
            using var verify = factory.Services.CreateScope();
            var db = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = await db.Orders.Include(value => value.Items).SingleAsync(value => value.StoreId == branchId);
            Assert.Equal(2, order.Items.Count);
            var customized = order.Items.Single(value => value.Quantity == 2);
            Assert.Equal(45000m, customized.UnitPrice);
            Assert.Contains("Trân châu", customized.Customization);
            Assert.Contains("Đường: Ít", customized.Customization);
            Assert.Contains("Đá: Không", customized.Customization);
            Assert.Equal(35000m, order.Items.Single(value => value.Quantity == 1).UnitPrice);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaleOptions.Where(value => value.Id == sizeId || value.Id == toppingId).ExecuteUpdateAsync(update => update.SetProperty(value => value.IsActive, false));
        }
    }

}
