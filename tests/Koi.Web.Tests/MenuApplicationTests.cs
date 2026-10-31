using System.Net;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Koi.Web.Tests.AdminTestClient;

namespace Koi.Web.Tests;

public class MenuApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ImportedMenusKeepAllLabelsOrderFilesAndNewTabs()
    {
        using var client = Anonymous(factory);
        var document = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/Home/Menu"));
        var countries = MenuCatalog.Countries.Where(value => value.Name == "VIETNAM").ToArray();
        Assert.Equal(countries.Select(value => value.Name), document.QuerySelectorAll(".menu-inner h2").Select(value => value.TextContent.Trim()));
        var links = document.QuerySelectorAll(".linkList a");
        Assert.Equal(countries.SelectMany(value => value.Menus).Select(value => value.Label), links.Select(value => value.QuerySelector(".store")!.TextContent));
        Assert.Equal(countries.SelectMany(value => value.Menus).Select(value => "/menus/" + value.File), links.Select(value => value.GetAttribute("href")));
        Assert.All(links, value => Assert.Equal("_blank", value.GetAttribute("target")));
    }

    [Fact]
    public async Task MenuCrudUpdatesPublicWebsiteAndProtectsRevisions()
    {
        using var admin = await Login(factory);
        var fields = new Dictionary<string, string> { ["Label"] = "Menu " + Guid.NewGuid().ToString("N"), ["MarketId"] = SeedGuid("Markets", 56).ToString(), ["SortOrder"] = "100", ["IsPublished"] = "false" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Menus/Create", fields, Image)).StatusCode);
        var menu = (await Find(fields["Label"]))!;
        using var client = Anonymous(factory);
        Assert.DoesNotContain(menu.Label, await client.GetStringAsync("/Home/Menu"));
        Assert.Equal(Image, await client.GetByteArrayAsync(menu.File));
        fields["Id"] = menu.Id.ToString(); fields["Revision"] = menu.Revision.ToString(); fields["IsPublished"] = "true";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Menus/Edit/{menu.Id}", fields)).StatusCode);
        Assert.Contains(menu.Label, await client.GetStringAsync("/Home/Menu"));
        fields["Label"] = "Stale overwrite";
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/Menus/Edit/{menu.Id}", fields, Image)).StatusCode);
        Assert.NotNull(await Find(menu.Label));
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Menus/Delete/{menu.Id}", new())).StatusCode);
        Assert.Null(await Find(menu.Label)); Assert.Equal(Image, await client.GetByteArrayAsync(menu.File));
    }

    [Theory]
    [InlineData("country")]
    [InlineData("missing-image")]
    [InlineData("invalid-image")]
    public async Task InvalidMenusAreNotSaved(string scenario)
    {
        using var admin = await Login(factory);
        var fields = new Dictionary<string, string> { ["Label"] = "Invalid " + Guid.NewGuid().ToString("N"), ["MarketId"] = scenario == "country" ? Guid.NewGuid().ToString() : SeedGuid("Markets", 56).ToString() };
        var image = scenario == "missing-image" ? null : scenario == "invalid-image" ? "<script>bad</script>"u8.ToArray() : Image;
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/Menus/Create", fields, image)).StatusCode);
        Assert.Null(await Find(fields["Label"]));
    }

    [Fact]
    public async Task MenuAdministrationRequiresRoleAndAntiforgery()
    {
        using var client = Anonymous(factory);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Admin/Menus")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Menus/Create", Form(new()))).StatusCode);
        using var admin = await Login(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync("/Admin/Menus/Create", Form(new()))).StatusCode);
    }

    private async Task<MenuEntry?> Find(string label)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Menus.AsNoTracking().SingleOrDefaultAsync(value => value.Label == label);
    }
}
