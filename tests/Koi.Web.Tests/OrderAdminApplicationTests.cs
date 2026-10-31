using System.Net;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

[Collection("Shop brand settings")]
public class OrderAdminApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task OnlyAdminCanViewAndAdvanceOrdersAndStaleUpdatesDoNotOverwrite()
    {
        using var anonymous = AdminTestClient.Anonymous(factory);
        Assert.Equal(HttpStatusCode.Redirect, (await anonymous.GetAsync("/Admin/Orders")).StatusCode);
        using var admin = await AdminTestClient.Login(factory);
        var order = new Order { StoreName = "Chi nhánh thử", StoreAddress = "Địa chỉ thử", CustomerName = "Khách thử", CustomerPhone = "0901234567", Items = [new OrderItem { DrinkName = "Trà", Quantity = 2, UnitPrice = 40000 }] };
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Orders.Add(order); await db.SaveChangesAsync();
        }
        var details = $"/Admin/Orders/Details/{order.Id}";
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(details)).StatusCode);
        var invalid = await AdminTestClient.Post(admin, "/Admin/Orders/ChangeStatus", new() { ["id"] = order.Id.ToString(), ["current"] = "0", ["next"] = "2" }, tokenUrl: details);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var first = await AdminTestClient.Post(admin, "/Admin/Orders/ChangeStatus", new() { ["id"] = order.Id.ToString(), ["current"] = "0", ["next"] = "1" }, tokenUrl: details);
        Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);
        var stale = await AdminTestClient.Post(admin, "/Admin/Orders/ChangeStatus", new() { ["id"] = order.Id.ToString(), ["current"] = "0", ["next"] = "3" }, tokenUrl: details);
        Assert.Equal(HttpStatusCode.Redirect, stale.StatusCode);
        using var verify = factory.Services.CreateScope();
        var database = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(OrderStatus.Preparing, (await database.Orders.FindAsync(order.Id))!.Status);
    }
}
