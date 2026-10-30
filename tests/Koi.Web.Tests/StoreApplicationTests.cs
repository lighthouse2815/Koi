using System.Net;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Koi.Web.Tests.AdminTestClient;

namespace Koi.Web.Tests;

public class StoreApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ImportedStoresKeepCountryAndCityOrdering()
    {
        using var client = Anonymous(factory);
        var snapshot = StoreCatalog.Snapshot;
        var queries = new List<(string Url, int[] Ids)> { ("/Home/Store", snapshot.Stores.Take(9).Select(value => value.Id).ToArray()) };
        queries.AddRange(snapshot.Countries.Select(value => ($"/Home/Store?c={value.Id}", value.StoreIds.Take(9).ToArray())));
        queries.AddRange(snapshot.Locations.Select(value => ($"/Home/Store?c={value.CountryId}&s={value.Id}", value.StoreIds.Take(9).ToArray())));
        foreach (var (url, ids) in queries)
        {
            var document = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync(url));
            Assert.Equal(ids, document.QuerySelectorAll(".storeList > li").Select(value => int.Parse(value.GetAttribute("data-store-id")!)));
        }
    }

    [Theory]
    [InlineData("/Admin/Stores")]
    [InlineData("/Admin/Geography?kind=countries")]
    [InlineData("/Admin/Geography?kind=cities")]
    [InlineData("/Admin/Geography?kind=brands")]
    public async Task AnonymousCannotManageCatalog(string url)
    {
        using var client = Anonymous(factory);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Stores/Create", Form(new()))).StatusCode);
    }

    private static Dictionary<string, string> Store(string name, int market = 56) => new()
    {
        ["Name"] = name, ["MarketId"] = market.ToString(), ["BrandId"] = "49", ["Address"] = "Địa chỉ kiểm tra",
        ["Phone"] = "0901234567", ["Hours"] = "09:00–22:00", ["SortOrder"] = "10000", ["CountryOrder"] = "10000", ["CityOrder"] = "10000", ["IsPublished"] = "false"
    };
    private async Task<StoreEntry?> Find(string name)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Stores.AsNoTracking().SingleOrDefaultAsync(value => value.Name == name);
    }

    [Fact]
    public async Task StoreCrudPersistsUploadsPublishingAndRejectsStaleUpdates()
    {
        using var admin = await Login(factory);
        var fields = Store("Store " + Guid.NewGuid().ToString("N"));
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Stores/Create", fields, Image)).StatusCode);
        var value = (await Find(fields["Name"]))!;
        Assert.True(value.Id >= 10000); Assert.False(value.IsPublished);
        Assert.Equal(Image, await admin.GetByteArrayAsync(value.Photo));
        using var client = Anonymous(factory);
        Assert.DoesNotContain(value.Name, await client.GetStringAsync("/Home/Store?c=56"));
        fields["Id"] = value.Id.ToString(); fields["Revision"] = value.Revision.ToString(); fields["IsPublished"] = "true";
        fields["CountryOrder"] = "0";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Stores/Edit/{value.Id}", fields)).StatusCode);
        Assert.Contains(value.Name, await client.GetStringAsync("/Home/Store?c=56"));
        fields["Address"] = "Stale overwrite";
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/Stores/Edit/{value.Id}", fields)).StatusCode);
        Assert.NotEqual("Stale overwrite", (await Find(value.Name))!.Address);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync($"/Admin/Stores/Delete/{value.Id}", Form(new()))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Stores/Delete/{value.Id}", new())).StatusCode);
        Assert.Null(await Find(value.Name));
    }

    [Theory]
    [InlineData("city")]
    [InlineData("brand")]
    [InlineData("url")]
    [InlineData("image")]
    public async Task InvalidStoreDataIsRejected(string scenario)
    {
        using var admin = await Login(factory); var fields = Store("Invalid " + Guid.NewGuid().ToString("N"));
        if (scenario == "city") fields["CityId"] = "71"; // Singapore city with Vietnam country
        if (scenario == "brand") fields["BrandId"] = "254"; // brand not assigned to Vietnam
        if (scenario == "url") fields["NavigationUrl"] = "javascript:alert(1)";
        var image = scenario == "image" ? "<script>bad</script>"u8.ToArray() : null;
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/Stores/Create", fields, image)).StatusCode);
        Assert.Null(await Find(fields["Name"]));
    }

    [Fact]
    public async Task GeographyCanBeCreatedEditedAndDeletedAndReferencesAreProtected()
    {
        using var admin = await Login(factory); var name = "Market " + Guid.NewGuid().ToString("N");
        var fields = new Dictionary<string, string> { ["Kind"] = "countries", ["Name"] = name, ["SortOrder"] = "1000", ["BrandIds"] = "49", ["SocialLinks"] = "Website | https://example.test/" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Geography/Create?kind=countries", fields)).StatusCode);
        int id;
        using (var scope = factory.Services.CreateScope())
            id = (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Markets.SingleAsync(value => value.Name == name)).Id;
        fields["Id"] = id.ToString(); fields["Revision"] = "1"; fields["Name"] += " updated";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Geography/Edit/{id}?kind=countries", fields)).StatusCode);
        using var client = Anonymous(factory);
        Assert.Contains(fields["Name"], await client.GetStringAsync($"/Home/Store?c={id}"));
        Assert.Contains("https://example.test/", await client.GetStringAsync($"/Home/Store?c={id}"));
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Geography/Delete/{id}?kind=countries", new() { ["kind"] = "countries" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, "/Admin/Geography/Delete/56?kind=countries", new() { ["kind"] = "countries" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, "/Admin/Geography/Delete/49?kind=brands", new() { ["kind"] = "brands" })).StatusCode);
    }
}
