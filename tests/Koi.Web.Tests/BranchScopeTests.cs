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
    public async Task ArchivedVietnamStoresAreNotAvailableForManagementOrOrdering()
    {
        using var admin = await AdminTestClient.Login(factory);
        using var customer = AdminTestClient.Anonymous(factory);
        Guid archivedId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            archivedId = await db.Database.SqlQuery<Guid>($"SELECT \"Id\" AS \"Value\" FROM \"ArchivedVietnamStores\" LIMIT 1").SingleAsync();
        }
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Stores/Edit/{archivedId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/Admin/Stores/Delete/{archivedId}")).StatusCode);
        var denied = await AdminTestClient.Post(customer, "/Order/SelectBranch", new() { ["storeId"] = archivedId.ToString() }, tokenUrl: "/Admin/Account/Login");
        Assert.Equal(HttpStatusCode.BadRequest, denied.StatusCode);
    }

}
