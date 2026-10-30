using System.Net;
using System.Security.Cryptography;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Koi.Web.Tests;

public class NewsApplicationFactory : WebApplicationFactory<Program>
{
    public string AdminEmail { get; } = $"admin-{Guid.NewGuid():N}@example.test";
    public string AdminPassword { get; } = "Aa1!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
    private readonly string uploads = Path.Combine(Path.GetTempPath(), "koi-tests-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(uploads);
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>(optional: true).Build();
        var connection = Environment.GetEnvironmentVariable("KOI_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connection))
        {
            var local = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Set KOI_TEST_CONNECTION_STRING for a dedicated PostgreSQL test database.");
            var options = new NpgsqlConnectionStringBuilder(local);
            options.Database += "_tests";
            connection = options.ConnectionString;
        }
        // Refuse a connection that could target the website's live database.
        if (!new NpgsqlConnectionStringBuilder(connection).Database!.EndsWith("_tests", StringComparison.Ordinal))
            throw new InvalidOperationException("The integration database name must end with _tests.");
        builder.UseEnvironment("Testing").UseWebRoot(uploads);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connection,
            ["Database:Initialize"] = "true",
            ["Admin:Email"] = AdminEmail,
            ["Admin:Password"] = AdminPassword
        }));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(uploads)) Directory.Delete(uploads, recursive: true);
    }
}

public class NewsApplicationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    private static byte[] Image => AdminTestClient.Image;
    private HttpClient Client() => AdminTestClient.Anonymous(factory);
    private static Task<string> Token(HttpClient client, string url) => AdminTestClient.Token(client, url);
    private Task<HttpClient> Admin() => AdminTestClient.Login(factory);
    private static Dictionary<string, string> Article(string title, bool published = false) => new()
    {
        ["Title"] = title, ["Category"] = "NEWS", ["Date"] = "2026-10-31",
        ["BodyHtml"] = "<p>Nội dung kiểm tra.</p>", ["IsPublished"] = published.ToString(),
        ["IsFeatured"] = "false", ["ShowOnHome"] = "false", ["HomeOrder"] = "0"
    };
    private static MultipartFormDataContent Form(Dictionary<string, string> fields, byte[]? image = null, string fileName = "test.png")
        => AdminTestClient.Form(fields, image, fileName);
    private async Task<NewsArticle?> Find(string title)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().NewsArticles.AsNoTracking().SingleOrDefaultAsync(a => a.Title == title);
    }
    private async Task<NewsArticle> Create(HttpClient client, Dictionary<string, string> fields)
    {
        fields["__RequestVerificationToken"] = await Token(client, "/Admin/News/Create");
        var result = await client.PostAsync("/Admin/News/Create", Form(fields, Image));
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
        return (await Find(fields["Title"]))!;
    }
    private static string Title() => "Integration " + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task ImportedArticlesDoNotContainRazorEscapedEmailAddresses()
    {
        using var scope = factory.Services.CreateScope();
        var rows = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().NewsArticles.AsNoTracking()
            .Where(article => article.Id < 10000).ToArrayAsync();
        Assert.Equal(13, rows.Length);
        Assert.All(rows, article => Assert.DoesNotContain("@@", article.BodyHtml));
        Assert.All(rows.Where(article => article.VideoId != null), article => Assert.Equal(11, article.VideoId!.Length));
    }

    [Theory]
    [InlineData(5253, true)]
    [InlineData(7931, false)]
    public async Task ImportedArticlesKeepTheirOriginalImageAndVideoCombination(int id, bool hasImage)
    {
        using var client = Client();
        var document = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync($"/Home/NewsDetail/{id}"));
        Assert.Equal(hasImage, document.QuerySelector(".banner img") != null);
        Assert.NotNull(document.QuerySelector(".banner iframe"));
    }

    [Fact]
    public async Task AnonymousCannotReadAdminOrMutateNews()
    {
        using var client = Client();
        var result = await client.GetAsync("/Admin/News");
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
        Assert.Contains("/Admin/Account/Login", result.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/News/Create", Form(Article(Title()), Image))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Admin/Account/Register")).StatusCode);
    }

    [Fact]
    public async Task AntiforgeryProtectsLoginAndAdminPosts()
    {
        using var client = await Admin();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Admin/News/Create", Form(Article(Title()), Image))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Admin/Account/Logout", new FormUrlEncodedContent([]))).StatusCode);
        using var anonymous = Client();
        Assert.Equal(HttpStatusCode.BadRequest, (await anonymous.PostAsync("/Admin/Account/Login", new FormUrlEncodedContent([]))).StatusCode);
    }

    [Fact]
    public async Task OrdinaryUserDoesNotHaveAdminAccess()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var email = $"reader-{Guid.NewGuid():N}@example.test";
        Assert.True((await users.CreateAsync(new IdentityUser { UserName = email, Email = email }, factory.AdminPassword)).Succeeded);
        using var client = Client();
        await client.PostAsync("/Admin/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Password"] = factory.AdminPassword,
            ["__RequestVerificationToken"] = await Token(client, "/Admin/Account/Login")
        }));
        var response = await client.GetAsync("/Admin/News");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task LoginDoesNotRedirectToAnExternalAddress()
    {
        using var client = Client();
        var result = await client.PostAsync("/Admin/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = factory.AdminEmail, ["Password"] = factory.AdminPassword,
            ["ReturnUrl"] = "https://example.invalid/", ["__RequestVerificationToken"] = await Token(client, "/Admin/Account/Login")
        }));
        Assert.Equal("/Admin", result.Headers.Location!.ToString());
    }

    [Fact]
    public async Task DraftIsPersistedAndOnlyVisibleInAuthorizedPreview()
    {
        using var admin = await Admin();
        var article = await Create(admin, Article(Title()));
        Assert.True(article.Id >= 10000);
        Assert.False(article.IsPublished);
        using var anonymous = Client();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/Home/NewsDetail/{article.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await anonymous.GetAsync($"/Home/NewsPreview/{article.Id}")).StatusCode);
        var preview = await admin.GetAsync($"/Home/NewsPreview/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.Contains(article.Title, await preview.Content.ReadAsStringAsync());
        var upload = await admin.GetAsync(article.Image);
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        Assert.Equal(Image, await upload.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task PublishingUpdatesPublicNewsAndHomepageAndSanitizesHtml()
    {
        using var admin = await Admin();
        var fields = Article(Title(), published: true);
        fields["ShowOnHome"] = "true";
        fields["HomeOrder"] = "0";
        fields["BodyHtml"] = "<p>Safe content</p><script>alert(1)</script><img src='x' onerror='alert(2)'>";
        fields["VideoUrl"] = "https://youtu.be/ooWUH3gPBqU";
        var article = await Create(admin, fields);
        Assert.DoesNotContain("<script", article.BodyHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", article.BodyHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("ooWUH3gPBqU", article.VideoId);
        using var client = Client();
        Assert.Contains(article.Title, await client.GetStringAsync($"/Home/NewsDetail/{article.Id}"));
        Assert.Contains(article.Title, await client.GetStringAsync("/Home/News"));
        Assert.Contains(article.Title, await client.GetStringAsync("/Home/Index"));
    }

    [Theory]
    [InlineData("invalid-category")]
    [InlineData("blank-title")]
    [InlineData("script-file")]
    [InlineData("unsafe-video")]
    public async Task InvalidInputIsRejectedWithoutCreatingRows(string scenario)
    {
        using var client = await Admin();
        var title = Title();
        var fields = Article(title);
        fields["__RequestVerificationToken"] = await Token(client, "/Admin/News/Create");
        if (scenario == "invalid-category") fields["Category"] = "ADMIN";
        if (scenario == "blank-title") fields["Title"] = " ";
        if (scenario == "unsafe-video") fields["VideoUrl"] = "https://example.invalid/embed/ooWUH3gPBqU";
        var result = await client.PostAsync("/Admin/News/Create", Form(fields, scenario == "script-file" ? "<script>bad</script>"u8.ToArray() : Image));
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Null(await Find(title));
    }

    [Fact]
    public async Task EditPersistsAndRejectsStaleRevision()
    {
        using var client = await Admin();
        var article = await Create(client, Article(Title()));
        var fields = Article(article.Title);
        fields["Id"] = article.Id.ToString(); fields["Revision"] = article.Revision.ToString();
        fields["BodyHtml"] = "<p>Updated content</p>";
        fields["__RequestVerificationToken"] = await Token(client, $"/Admin/News/Edit/{article.Id}");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Admin/News/Edit/{article.Id}", Form(fields))).StatusCode);
        Assert.Contains("Updated content", (await Find(article.Title))!.BodyHtml);
        fields["BodyHtml"] = "<p>Stale overwrite</p>";
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/Admin/News/Edit/{article.Id}", Form(fields))).StatusCode);
        Assert.DoesNotContain("Stale overwrite", (await Find(article.Title))!.BodyHtml);
    }

    [Fact]
    public async Task DeleteRequiresPostAndRemovesOnlyTheSelectedArticle()
    {
        using var client = await Admin();
        var article = await Create(client, Article(Title(), true));
        var token = await Token(client, $"/Admin/News/Delete/{article.Id}");
        Assert.NotNull(await Find(article.Title));
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/Admin/News/Delete/{article.Id}", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }))).StatusCode);
        Assert.Null(await Find(article.Title));
        using var anonymous = Client();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/Home/NewsDetail/{article.Id}")).StatusCode);
    }
}
