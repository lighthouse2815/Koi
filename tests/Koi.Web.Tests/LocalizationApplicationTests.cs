using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Koi.Web.Tests.AdminTestClient;

namespace Koi.Web.Tests;

// These classes publish temporary drinks into the same integration database.
[Collection("Drink catalog")]
public class LocalizationApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    private static async Task<IDocument> Page(HttpClient client, string path) => await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync(path));
    private static Task<HttpResponseMessage> Language(HttpClient client, string culture, string returnUrl = "/Home/Index")
        => Post(client, "/Home/Language", new() { ["culture"] = culture, ["returnUrl"] = returnUrl }, tokenUrl: "/");

    [Fact]
    public async Task CoverOffersVietnameseAndEnglishAndSelectionPersistsAcrossPages()
    {
        using var client = Anonymous(factory); var cover = await Page(client, "/");
        Assert.Equal(2, cover.QuerySelectorAll(".cover-languages form").Length);
        Assert.Contains("TIẾNG VIỆT", cover.Body!.TextContent); Assert.DoesNotContain("中文", cover.Body.TextContent);
        var response = await Language(client, "vi", $"/Home/Store?c={SeedGuid("Markets", 56)}&s={SeedGuid("StoreCities", 109)}&page=2");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/Home/Store?c={SeedGuid("Markets", 56)}&s={SeedGuid("StoreCities", 109)}&page=2", response.Headers.Location!.ToString());
        var page = await Page(client, $"/Home/Store?c={SeedGuid("Markets", 56)}");
        Assert.Equal("vi", page.DocumentElement.GetAttribute("lang")); Assert.Contains("CỬA HÀNG", page.QuerySelector("h1")!.TextContent);
        Assert.Contains("VIỆT NAM", page.Body!.TextContent); Assert.Contains("Ha Noi", page.Body.TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Redirect, (await Language(client, "en")).StatusCode);
        page = await Page(client, $"/Home/Store?c={SeedGuid("Markets", 56)}");
        Assert.Equal("en", page.DocumentElement.GetAttribute("lang")); Assert.Contains("STORE", page.QuerySelector("h1")!.TextContent);
        Assert.Contains("VIETNAM", page.Body!.TextContent);
    }

    [Fact]
    public async Task LanguageChangeRequiresAntiforgeryAndRejectsInvalidCultureOrExternalRedirects()
    {
        using var client = Anonymous(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Home/Language", Form(new() { ["culture"] = "vi" }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Language(client, "zh")).StatusCode);
        Assert.Equal("/Home/Index", (await Language(client, "vi", "https://example.invalid/")).Headers.Location!.ToString());
        Assert.Equal("/Home/Index", (await Language(client, "en", "//example.invalid/")).Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("About", "Giới thiệu")]
    [InlineData("OurTea", "Trà của KOI")]
    [InlineData("News", "Tin tức")]
    [InlineData("Recruit", "Tuyển dụng")]
    [InlineData("Impact", "Tác động tích cực")]
    [InlineData("Responsibility", "Trách nhiệm")]
    [InlineData("Precision", "Sự chuẩn xác")]
    [InlineData("People", "Con người")]
    [InlineData("Connections", "Kết nối")]
    [InlineData("Contact", "Liên hệ")]
    [InlineData("Menu", "Thực đơn")]
    public async Task PublicPagesRenderVietnameseOnTheServer(string action, string title)
    {
        using var client = Anonymous(factory); var page = await Page(client, $"/Home/{action}?culture=vi");
        Assert.Equal("vi", page.DocumentElement.GetAttribute("lang")); Assert.StartsWith(title, page.Title);
        Assert.Contains("TRANG CHỦ", page.QuerySelector(".fan-links")!.TextContent);
        Assert.DoesNotContain("__text", page.QuerySelector("main")!.TextContent);
        Assert.NotNull(page.QuerySelector(".language-switcher input[name=__RequestVerificationToken]"));
    }

    [Fact]
    public async Task DatabaseContentUsesOriginalFieldsInBothLanguages()
    {
        using var client = Anonymous(factory);
        var english = await Page(client, $"/Home/NewsDetail/{SeedGuid("NewsArticles", 7931)}?culture=en");
        var vietnamese = await Page(client, $"/Home/NewsDetail/{SeedGuid("NewsArticles", 7931)}?culture=vi");
        Assert.Equal("With Care", english.QuerySelector("h1")!.TextContent);
        Assert.Equal(english.QuerySelector("h1")!.TextContent, vietnamese.QuerySelector("h1")!.TextContent);
        Assert.Equal(english.QuerySelector(".detail-area")!.TextContent, vietnamese.QuerySelector(".detail-area")!.TextContent);
        var englishTea = await Page(client, "/Home/OurTea?culture=en");
        var vietnameseTea = await Page(client, "/Home/OurTea?culture=vi");
        Assert.Equal(englishTea.QuerySelectorAll(".tea-product-details .en").Select(node => node.TextContent), vietnameseTea.QuerySelectorAll(".tea-product-details .en").Select(node => node.TextContent));
    }

    [Fact]
    public async Task EditingDrinkKeepsExistingVietnameseDataWithoutDisplayingIt()
    {
        var name = "LocaleDrink " + Guid.NewGuid().ToString("N");
        Guid id;
        using (var scope = factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var drink = new Drink { Name = name, NameVi = "Tên cũ", DescriptionVi = "Mô tả cũ", CategoryId = SeedGuid("DrinkCategories", 45), IsPublished = true };
            database.Drinks.Add(drink);
            await database.SaveChangesAsync();
            id = drink.Id;
        }
        using var admin = await Login(factory);
        var editor = await Page(admin, $"/Admin/Drinks/Edit/{id}");
        Assert.Null(editor.QuerySelector("input[name=NameVi]"));
        var fields = new Dictionary<string, string> { ["Id"] = id.ToString(), ["Revision"] = "1", ["Name"] = name + " edited", ["CategoryId"] = SeedGuid("DrinkCategories", 45).ToString(), ["IsPublished"] = "true" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Edit/{id}", fields)).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var drink = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Drinks.SingleAsync(value => value.Id == id);
            Assert.Equal("Tên cũ", drink.NameVi);
            Assert.Equal("Mô tả cũ", drink.DescriptionVi);
        }
        using var client = Anonymous(factory);
        foreach (var culture in new[] { "vi", "en" })
        {
            var page = await Page(client, "/Home/OurTea?culture=" + culture);
            Assert.Contains(name + " edited", page.QuerySelector(".drinkWrapList")!.TextContent);
            Assert.DoesNotContain("Tên cũ", page.QuerySelector(".drinkWrapList")!.TextContent);
        }
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Delete/{id}", new())).StatusCode);
    }
}
