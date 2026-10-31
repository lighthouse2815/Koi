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
        var chainBrand = snapshot.Brands.Single(value => value.Name == "KOI Thé").Id;
        var vietnam = snapshot.Countries.Single(value => value.Name == "VIETNAM");
        var branchIds = snapshot.Stores.Where(value => value.BrandId == chainBrand && value.CountryId == vietnam.Id).Select(value => value.Id).ToHashSet();
        var firstPage = vietnam.StoreIds.Where(branchIds.Contains).Take(9).Select(id => SeedGuid("Stores", id)).ToArray();
        var queries = new List<(string Url, Guid[] Ids)> { ("/Home/Store", firstPage), ($"/Home/Store?c={SeedGuid("Markets", 51)}", firstPage) };
        queries.AddRange(snapshot.Locations.Where(value => value.CountryId == vietnam.Id).Select(value => ($"/Home/Store?s={SeedGuid("StoreCities", value.Id)}", value.StoreIds.Where(branchIds.Contains).Take(9).Select(id => SeedGuid("Stores", id)).ToArray())));
        foreach (var (url, ids) in queries)
        {
            var document = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync(url));
            Assert.Equal(ids, document.QuerySelectorAll(".storeList > li").Select(value => Guid.Parse(value.GetAttribute("data-store-id")!)));
        }
    }

    [Fact]
    public async Task AboutLinksPointToExistingMarketGuids()
    {
        using var client = Anonymous(factory);
        var page = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/Home/About"));
        Assert.NotNull(page.QuerySelector("a[href=\"/Home/Store\"]"));
        Assert.Empty(page.QuerySelectorAll("a[href*=\"/Home/Store?c=\"]"));
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
        ["Name"] = name, ["MarketId"] = SeedGuid("Markets", market).ToString(), ["BrandId"] = SeedGuid("TeaBrands", 49).ToString(), ["Address"] = "Địa chỉ kiểm tra",
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
        Assert.NotEqual(Guid.Empty, value.Id); Assert.False(value.IsPublished);
        Assert.Equal(Image, await admin.GetByteArrayAsync(value.Photo));
        using var client = Anonymous(factory);
        Assert.DoesNotContain(value.Name, await client.GetStringAsync($"/Home/Store?c={SeedGuid("Markets", 56)}"));
        fields["Id"] = value.Id.ToString(); fields["Revision"] = value.Revision.ToString(); fields["IsPublished"] = "true";
        fields["CountryOrder"] = "0";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Stores/Edit/{value.Id}", fields)).StatusCode);
        Assert.Contains(value.Name, await client.GetStringAsync($"/Home/Store?c={SeedGuid("Markets", 56)}"));
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
        if (scenario == "city") fields["CityId"] = SeedGuid("StoreCities", 71).ToString(); // Singapore city with Vietnam country
        if (scenario == "brand") fields["BrandId"] = SeedGuid("TeaBrands", 254).ToString(); // brand not assigned to Vietnam
        if (scenario == "url") fields["NavigationUrl"] = "javascript:alert(1)";
        var image = scenario == "image" ? "<script>bad</script>"u8.ToArray() : null;
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/Stores/Create", fields, image)).StatusCode);
        Assert.Null(await Find(fields["Name"]));
    }

    [Fact]
    public async Task GeographyCanBeCreatedEditedAndDeletedAndReferencesAreProtected()
    {
        using var admin = await Login(factory); var name = "Market " + Guid.NewGuid().ToString("N");
        var fields = new Dictionary<string, string> { ["Kind"] = "countries", ["Name"] = name, ["SortOrder"] = "1000", ["BrandIds"] = SeedGuid("TeaBrands", 49).ToString(), ["SocialLinks"] = "Website | https://example.test/" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Geography/Create?kind=countries", fields)).StatusCode);
        Guid id;
        using (var scope = factory.Services.CreateScope())
            id = (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Markets.SingleAsync(value => value.Name == name)).Id;
        fields["Id"] = id.ToString(); fields["Revision"] = "1"; fields["Name"] += " updated";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Geography/Edit/{id}?kind=countries", fields)).StatusCode);
        using var client = Anonymous(factory);
        Assert.DoesNotContain(fields["Name"], await client.GetStringAsync($"/Home/Store?c={id}"));
        using (var scope = factory.Services.CreateScope())
            Assert.Contains((await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Markets.Include(value => value.SocialLinks).SingleAsync(value => value.Id == id)).SocialLinks, link => link.Url == "https://example.test/");
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Geography/Delete/{id}?kind=countries", new() { ["kind"] = "countries" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/Geography/Delete/{SeedGuid("Markets", 56)}?kind=countries", new() { ["kind"] = "countries" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/Geography/Delete/{SeedGuid("TeaBrands", 49)}?kind=brands", new() { ["kind"] = "brands" })).StatusCode);
    }
}
