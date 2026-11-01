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
        using var client = AdminTestClient.Anonymous(factory);
        _ = await client.GetAsync("/Order/Index");
        Guid branchId, drinkId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new DrinkCategory { Name = "Danh mục " + Guid.NewGuid(), Image = "test", IsPublished = true };
            var branch = new StoreEntry { Name = "Chi nhánh đặt hàng " + Guid.NewGuid(), Address = "Địa chỉ kiểm thử", AcceptsOrders = true, IsPublished = true };
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
        var confirmation = await client.GetAsync(checkout.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
        Assert.Contains("Đã gửi đơn hàng", WebUtility.HtmlDecode(await confirmation.Content.ReadAsStringAsync()));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync(checkout.Headers.Location)).StatusCode);
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
    [Fact]
    public async Task CustomizationsArePricedAndSavedForEachCup()
    {
        using var client = AdminTestClient.Anonymous(factory);
        Guid branchId, drinkId, sizeId, toppingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new DrinkCategory { Name = "Danh mục lựa chọn " + Guid.NewGuid(), Image = "test" };
            var drink = new Drink { Name = "Trà lựa chọn " + Guid.NewGuid(), Category = category, Price = 30000 };
            var branch = new StoreEntry { Name = "Chi nhánh lựa chọn " + Guid.NewGuid(), Address = "Địa chỉ", AcceptsOrders = true };
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

    [Fact]
    public async Task SelectedDrinkAndCategoryRemainVisibleAfterChoosingBranch()
    {
        using var client = AdminTestClient.Anonymous(factory);
        Guid branchId, categoryId, firstId, secondId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new DrinkCategory { Name = "Menu test " + Guid.NewGuid(), Image = "/images/category.jpg" };
            var first = new Drink { Name = "First " + Guid.NewGuid(), Category = category, Image = "/images/first.jpg", Price = 45000 };
            var second = new Drink { Name = "Second " + Guid.NewGuid(), Category = category, Price = null };
            var branch = new StoreEntry { Name = "Branch " + Guid.NewGuid(), AcceptsOrders = true };
            db.AddRange(first, second, branch); await db.SaveChangesAsync();
            categoryId = category.Id; branchId = branch.Id; firstId = first.Id; secondId = second.Id;
        }
        var categoryPage = await client.GetStringAsync($"/Order/Index?categoryId={categoryId}");
        Assert.Contains(firstId.ToString(), categoryPage);
        Assert.Contains(secondId.ToString(), categoryPage);
        Assert.Contains("/images/category.jpg", categoryPage);
        Assert.Contains("Chưa mở bán", WebUtility.HtmlDecode(categoryPage));
        var selectedPage = await client.GetStringAsync($"/Order/Index?drinkId={firstId}");
        Assert.Contains("/images/first.jpg", selectedPage);
        Assert.DoesNotContain(secondId.ToString(), selectedPage);
        var response = await AdminTestClient.Post(client, "/Order/SelectBranch", new() { ["storeId"] = branchId.ToString(), ["drinkId"] = firstId.ToString() }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(firstId.ToString(), response.Headers.Location!.ToString());
        var afterBranch = await client.GetStringAsync(response.Headers.Location);
        Assert.Contains(firstId.ToString(), afterBranch);
        Assert.DoesNotContain(secondId.ToString(), afterBranch);
        Assert.Contains("/Order/Add", afterBranch);
        var add = await AdminTestClient.Post(client, "/Order/Add", new() { ["id"] = firstId.ToString(), ["drinkId"] = firstId.ToString(), ["quantity"] = "1" }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, add.StatusCode);
        Assert.Contains($"drinkId={firstId}", add.Headers.Location!.ToString());
        Assert.EndsWith("#drinks", add.Headers.Location.ToString());
        var cartPage = await client.GetStringAsync(add.Headers.Location);
        Assert.Contains("cart-line", cartPage);
        Assert.DoesNotContain(secondId.ToString(), cartPage);
    }

}
