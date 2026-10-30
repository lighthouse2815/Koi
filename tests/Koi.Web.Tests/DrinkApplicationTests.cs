using System.Net;
using System.Text.Json;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Koi.Web.Tests.AdminTestClient;

namespace Koi.Web.Tests;

[Collection("Drink catalog")]
public class DrinkApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ImportedCatalogPreservesNamesDescriptionsRegionsImagesAndHomeCards()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("Koi.Web.Data.DrinksSeed.json")!;
        using var seed = await JsonDocument.ParseAsync(stream);
        using var client = Anonymous(factory);
        var page = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/Home/OurTea"));
        var categories = seed.RootElement.GetProperty("categories").EnumerateArray().ToArray();
        Assert.Equal(7, categories.Length);
        Assert.Equal(54, page.QuerySelectorAll(".drinkWrapList .drinkList > li").Length);
        foreach (var category in categories)
        {
            var section = page.QuerySelector($".drinkWrapList > li[id='{category.GetProperty("id").GetInt32()}']")!;
            Assert.Equal(category.GetProperty("name").GetString(), section.QuerySelector(".title .en")!.TextContent);
            var rows = section.QuerySelectorAll(".drinkList > li"); var drinks = category.GetProperty("drinks").EnumerateArray().ToArray();
            Assert.Equal(drinks.Length, rows.Length);
            for (var index = 0; index < rows.Length; index++)
            {
                Assert.Equal(drinks[index].GetProperty("name").GetString(), rows[index].QuerySelector(".en")!.TextContent);
                Assert.Equal(drinks[index].GetProperty("description").GetString(), rows[index].QuerySelector(".content")!.TextContent);
                Assert.Equal(drinks[index].GetProperty("region").GetString(), rows[index].QuerySelector(".region")?.TextContent.TrimStart('/', ' ') ?? "");
            }
        }
        Assert.Equal(4, page.QuerySelectorAll(".new-drink > li").Length);
        var home = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/Home/Index"));
        Assert.Equal(new[] { "45", "4", "44", "5", "43", "6" }, home.QuerySelectorAll(".drink-card").Select(value => value.GetAttribute("href")!.Split('#').Last()));
    }

    [Theory]
    [InlineData("Drinks")]
    [InlineData("DrinkCategories")]
    [InlineData("FeaturedDrinks")]
    public async Task CatalogAdministrationRequiresRoleAndAntiforgery(string controller)
    {
        using var client = Anonymous(factory);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync($"/Admin/{controller}")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Admin/{controller}/Create", Form(new()))).StatusCode);
        using var admin = await Login(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync($"/Admin/{controller}/Create", Form(new()))).StatusCode);
    }

    [Fact]
    public async Task CategoryAndDrinkCrudPublishToOurTeaAndHomeAndProtectReferencesAndRevisions()
    {
        using var admin = await Login(factory); using var client = Anonymous(factory);
        var categoryName = "Category " + Guid.NewGuid().ToString("N");
        var categoryFields = new Dictionary<string, string> { ["Name"] = categoryName, ["SortOrder"] = "100", ["HomeOrder"] = "100", ["IsPublished"] = "true", ["ShowOnHome"] = "true" };
        categoryFields["__RequestVerificationToken"] = await Token(admin, "/Admin/DrinkCategories/Create");
        using var categoryForm = Form(categoryFields, Image);
        categoryForm.Add(new ByteArrayContent(Image), "HomeImageUpload", "home.png"); categoryForm.Add(new ByteArrayContent(Image), "HomeIconUpload", "icon.png");
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/Admin/DrinkCategories/Create", categoryForm)).StatusCode);
        int categoryId;
        using (var scope = factory.Services.CreateScope())
            categoryId = (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().DrinkCategories.SingleAsync(value => value.Name == categoryName)).Id;
        Assert.Contains(categoryName, await client.GetStringAsync("/Home/Index"));
        var fields = new Dictionary<string, string> { ["Name"] = "Drink " + Guid.NewGuid().ToString("N"), ["CategoryId"] = categoryId.ToString(), ["Description"] = "<script>text, not HTML</script>", ["Region"] = "Vietnam", ["IsPublished"] = "false", ["SortOrder"] = "1" };
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, "/Admin/Drinks/Create", fields, Image)).StatusCode);
        Drink drink;
        using (var scope = factory.Services.CreateScope())
            drink = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Drinks.AsNoTracking().SingleAsync(value => value.Name == fields["Name"]);
        Assert.Equal(Image, await client.GetByteArrayAsync(drink.Image));
        Assert.DoesNotContain(drink.Name, await client.GetStringAsync("/Home/OurTea"));
        fields["Id"] = drink.Id.ToString(); fields["Revision"] = "1"; fields["IsPublished"] = "true";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Edit/{drink.Id}", fields)).StatusCode);
        var html = await client.GetStringAsync("/Home/OurTea"); Assert.Contains(drink.Name, html); Assert.Contains("&lt;script&gt;text, not HTML&lt;/script&gt;", html);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/Drinks/Edit/{drink.Id}", fields)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/DrinkCategories/Delete/{categoryId}", new())).StatusCode);
        categoryFields["Id"] = categoryId.ToString(); categoryFields["Revision"] = "1"; categoryFields["IsPublished"] = "false";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/DrinkCategories/Edit/{categoryId}", categoryFields)).StatusCode);
        Assert.DoesNotContain(categoryName, await client.GetStringAsync("/Home/Index"));
        Assert.DoesNotContain(drink.Name, await client.GetStringAsync("/Home/OurTea"));
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/DrinkCategories/Edit/{categoryId}", categoryFields)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/Drinks/Delete/{drink.Id}", new())).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/DrinkCategories/Delete/{categoryId}", new())).StatusCode);
    }

    [Fact]
    public async Task FeaturedDrinkCrudPersistsBothImagesAndPublication()
    {
        using var admin = await Login(factory); using var client = Anonymous(factory);
        var name = "Feature " + Guid.NewGuid().ToString("N");
        var fields = new Dictionary<string, string> { ["Name"] = name, ["Region"] = "Vietnam", ["IsPublished"] = "false", ["SortOrder"] = "100" };
        fields["__RequestVerificationToken"] = await Token(admin, "/Admin/FeaturedDrinks/Create");
        using var form = Form(fields, Image); form.Add(new ByteArrayContent(Image), "BackgroundUpload", "background.png");
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/Admin/FeaturedDrinks/Create", form)).StatusCode);
        FeaturedDrink feature;
        using (var scope = factory.Services.CreateScope())
            feature = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().FeaturedDrinks.AsNoTracking().SingleAsync(value => value.Name == name);
        Assert.Equal(Image, await client.GetByteArrayAsync(feature.Image)); Assert.Equal(Image, await client.GetByteArrayAsync(feature.BackgroundImage));
        Assert.DoesNotContain(name, await client.GetStringAsync("/Home/OurTea"));
        fields["Id"] = feature.Id.ToString(); fields["Revision"] = "1"; fields["IsPublished"] = "true";
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/FeaturedDrinks/Edit/{feature.Id}", fields)).StatusCode);
        Assert.Contains(name, await client.GetStringAsync("/Home/OurTea"));
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, $"/Admin/FeaturedDrinks/Edit/{feature.Id}", fields)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await Post(admin, $"/Admin/FeaturedDrinks/Delete/{feature.Id}", new())).StatusCode);
        Assert.DoesNotContain(name, await client.GetStringAsync("/Home/OurTea"));
    }

    [Theory]
    [InlineData("missing-category")]
    [InlineData("invalid-image")]
    [InlineData("blank-name")]
    public async Task InvalidDrinkInputIsRejected(string scenario)
    {
        using var admin = await Login(factory);
        var fields = new Dictionary<string, string> { ["Name"] = scenario == "blank-name" ? " " : "Invalid " + Guid.NewGuid().ToString("N"), ["CategoryId"] = scenario == "missing-category" ? "9999999" : "45" };
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/Drinks/Create", fields, scenario == "invalid-image" ? "bad"u8.ToArray() : null)).StatusCode);
    }

    [Fact]
    public async Task CategoryHomeCardsAndFeaturedDrinksRequireTheirImages()
    {
        using var admin = await Login(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/DrinkCategories/Create", new() { ["Name"] = "Invalid", ["ShowOnHome"] = "true" }, Image)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(admin, "/Admin/FeaturedDrinks/Create", new() { ["Name"] = "Invalid" }, Image)).StatusCode);
    }
}
