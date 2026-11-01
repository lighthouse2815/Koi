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
        Assert.Contains("VIỆT NAM", page.Body!.TextContent); Assert.Contains("Hà Nội", page.Body.TextContent);
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
    public async Task ImportedVietnameseNewsAndDrinksKeepTheirEnglishVersions()
    {
        using var client = Anonymous(factory);
        var english = await Page(client, $"/Home/NewsDetail/{SeedGuid("NewsArticles", 7931)}?culture=en");
        var vietnamese = await Page(client, $"/Home/NewsDetail/{SeedGuid("NewsArticles", 7931)}?culture=vi");
        Assert.Equal("With Care", english.QuerySelector("h1")!.TextContent);
        Assert.Equal("Bằng sự tận tâm", vietnamese.QuerySelector("h1")!.TextContent);
        Assert.Contains("Mỗi ly trà", vietnamese.QuerySelector(".detail-area")!.TextContent);
        var tea = await Page(client, "/Home/OurTea?culture=vi");
        Assert.Contains("Trà sữa trân châu hoàng kim", tea.QuerySelector(".drinkWrapList")!.TextContent);
        Assert.Equal(28, tea.QuerySelectorAll(".drinkWrapList .drinkList > li").Length);
        Assert.Equal(4, tea.QuerySelectorAll(".new-drink > li").Length);
    }

    [Fact]
    public async Task AdminCanSaveBothLanguagesAndVietnameseHtmlIsSanitized()
    {
        using var admin = await Login(factory); var title = "Locale " + Guid.NewGuid().ToString("N");
        var response = await Post(admin, "/Admin/News/Create", new()
        {
            ["Title"] = title, ["TitleVi"] = "Bài viết tiếng Việt", ["BodyHtml"] = "<p>English article</p>",
            ["BodyHtmlVi"] = "<p>Nội dung tiếng Việt</p><script>alert(1)</script><img src=x onerror=alert(1)>",
            ["Category"] = "NEWS", ["Date"] = "2026-10-31", ["IsPublished"] = "true"
        }, Image);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Guid id;
        using (var scope = factory.Services.CreateScope())
        {
            var article = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().NewsArticles.SingleAsync(value => value.Title == title);
            id = article.Id; Assert.DoesNotContain("<script", article.BodyHtmlVi); Assert.DoesNotContain("onerror", article.BodyHtmlVi);
        }
        Assert.Contains(title, (await Page(admin, "/Admin/News?search=" + Uri.EscapeDataString("Bài viết tiếng Việt"))).Body!.TextContent);
        using var client = Anonymous(factory);
        Assert.Equal("Bài viết tiếng Việt", (await Page(client, $"/Home/NewsDetail/{id}?culture=vi")).QuerySelector("h1")!.TextContent);
        Assert.Equal(title, (await Page(client, $"/Home/NewsDetail/{id}?culture=en")).QuerySelector("h1")!.TextContent);
        var editor = await Page(admin, $"/Admin/News/Edit/{id}");
        Assert.Equal("Bài viết tiếng Việt", editor.QuerySelector("input[name=TitleVi]")!.GetAttribute("value"));
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/News/Delete/{id}", new())).StatusCode);
    }

    [Fact]
    public async Task AdminDrinkTranslationAndEnglishFallbackAreVisibleInTheCorrectLanguage()
    {
        using var admin = await Login(factory); var name = "LocaleDrink " + Guid.NewGuid().ToString("N");
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Drinks/Create", new()
        {
            ["Name"] = name, ["NameVi"] = "Trà thử tiếng Việt", ["DescriptionVi"] = "Mô tả tiếng Việt", ["RegionVi"] = "Việt Nam", ["CategoryId"] = SeedGuid("DrinkCategories", 45).ToString(), ["IsPublished"] = "true"
        })).StatusCode);
        Guid id;
        using (var scope = factory.Services.CreateScope())
            id = (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Drinks.SingleAsync(value => value.Name == name)).Id;
        using var client = Anonymous(factory);
        Assert.Contains("Trà thử tiếng Việt", (await Page(client, "/Home/OurTea?culture=vi")).QuerySelector(".drinkWrapList")!.TextContent);
        Assert.Contains(name, (await Page(admin, "/Admin/Drinks?search=" + Uri.EscapeDataString("Trà thử tiếng Việt"))).Body!.TextContent);
        Assert.Contains(name, (await Page(client, "/Home/OurTea?culture=en")).QuerySelector(".drinkWrapList")!.TextContent);
        var fields = new Dictionary<string, string> { ["Id"] = id.ToString(), ["Revision"] = "1", ["Name"] = name, ["CategoryId"] = SeedGuid("DrinkCategories", 45).ToString(), ["IsPublished"] = "true" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Edit/{id}", fields)).StatusCode);
        Assert.Contains(name, (await Page(client, "/Home/OurTea?culture=vi")).QuerySelector(".drinkWrapList")!.TextContent);
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Delete/{id}", new())).StatusCode);
    }
}
