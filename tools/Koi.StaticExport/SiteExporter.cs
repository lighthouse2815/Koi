using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Koi.StaticExport;

internal sealed class SiteExporter
{
    private static readonly string[] Actions = ["Index", "About", "OurTea", "Recruit", "News", "Store", "Impact", "Responsibility", "Precision", "People", "Connections", "Contact", "Menu"];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly string root;
    private readonly string output;
    private readonly Uri source;
    private readonly string staging;
    private readonly HttpClient client = new(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });
    private readonly ConcurrentDictionary<string, byte> uploads = new(StringComparer.Ordinal);
    private readonly ResourceManager labels;

    public SiteExporter(string[] args)
    {
        var options = new Dictionary<string, string>();
        for (var index = 0; index < args.Length; index += 2)
        {
            if (index + 1 == args.Length || args[index] is not ("--source" or "--output"))
                throw new ArgumentException("Dùng --source http://localhost:5118 --output fe-demo.");
            options.Add(args[index], args[index + 1]);
        }
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Koi.Web.csproj"))) directory = directory.Parent;
        root = directory?.FullName ?? throw new InvalidOperationException("Chạy lệnh trong repository Koi.");
        output = Path.GetFullPath(options.GetValueOrDefault("--output", "fe-demo"), root);
        // This tool only replaces its own generated output, never source code or database files.
        if (output != Path.Combine(root, "fe-demo")) throw new ArgumentException("Thư mục xuất phải là fe-demo ở gốc repository.");
        if (Directory.Exists(output) && !File.Exists(Path.Combine(output, "static-site.json")))
            throw new InvalidOperationException("fe-demo đã tồn tại và không phải bản xuất của công cụ này.");
        source = new Uri(options.GetValueOrDefault("--source", "http://localhost:5118").TrimEnd('/') + "/");
        if (source.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(source.UserInfo)) throw new ArgumentException("Địa chỉ nguồn không hợp lệ.");
        staging = Path.Combine(root, "artifacts", "static-export", Guid.NewGuid().ToString("N"));
        var assembly = typeof(ApplicationDbContext).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(name => name.EndsWith(".Localization.SiteText.resources", StringComparison.Ordinal));
        labels = new ResourceManager(resource[..^".resources".Length], assembly);
    }

    public async Task RunAsync()
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets(typeof(ApplicationDbContext).Assembly, optional: true).AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cấu hình ConnectionStrings:DefaultConnection trước khi xuất.");
        await using var database = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
        await using var transaction = await database.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        await database.Database.ExecuteSqlRawAsync("SET TRANSACTION READ ONLY");
        var articles = await database.NewsArticles.AsNoTracking().Where(value => value.IsPublished)
            .OrderByDescending(value => value.IsFeatured).ThenByDescending(value => value.Date).ThenByDescending(value => value.Id).ToArrayAsync();
        var featuredId = articles.FirstOrDefault()?.Id;
        var listedNews = articles.Where(value => value.Id != featuredId).OrderByDescending(value => value.Date).ThenByDescending(value => value.Id).ToArray();
        var stores = await database.Stores.AsNoTracking().Where(value => value.IsPublished).OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var cities = await database.StoreCities.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var counts = new
        {
            news = articles.Length, stores = stores.Length,
            menus = await database.Menus.CountAsync(value => value.IsPublished),
            drinkCategories = await database.DrinkCategories.CountAsync(value => value.IsPublished),
            drinks = await database.Drinks.CountAsync(value => value.IsPublished && value.Category.IsPublished),
            featuredDrinks = await database.FeaturedDrinks.CountAsync(value => value.IsPublished)
        };

        Directory.CreateDirectory(staging);
        var cover = await Get("/", "en");
        Prepare(cover, "en", "/", isCover: true);
        await Write("index.html", Html(cover));
        foreach (var language in new[] { "en", "vi" })
        {
            Console.WriteLine($"Xuất {language}: trang công khai, {articles.Length} bài, {stores.Length} cửa hàng...");
            foreach (var action in Actions)
            {
                var page = await Get("/Home/" + action, language);
                Prepare(page, language, "/Home/" + action);
                await Write($"{language}/Home/{action}/index.html", Html(page));
            }
            foreach (var article in articles)
            {
                var page = await Get($"/Home/NewsDetail/{article.Id}", language);
                Prepare(page, language, $"/Home/NewsDetail/{article.Id}");
                await Write($"{language}/Home/NewsDetail/{article.Id}/index.html", Html(page));
            }
            var cards = await CaptureCards("Store", ".storeList > li", stores.Length, 9, language,
                element => Guid.Parse(element.GetAttribute("data-store-id")!));
            if (!cards.Keys.Order().SequenceEqual(stores.Select(value => value.Id).Order())) throw new InvalidOperationException("Dữ liệu cửa hàng trên web và database không khớp.");
            await WriteJson($"data/stores-{language}.json", new
            {
                locations = cities.Select(value => new { value.Id, name = value.Name }),
                stores = stores.Select(value => new { value.Id, locationId = value.CityId, value.CountryOrder, locationOrder = value.CityOrder, html = cards[value.Id] }),
                labels = Text(language, "All__text", "prev", "next", "Page {0} of {1}", "Store pages", "No stores match these filters. Choose another location.")
            });
            var newsCards = await CaptureCards("News", ".newsList > li", listedNews.Length, 5, language, element =>
                Guid.Parse(new Uri(source, element.QuerySelector("a")!.GetAttribute("href")!).AbsolutePath.TrimEnd('/').Split('/').Last()));
            if (!newsCards.Keys.Order().SequenceEqual(listedNews.Select(value => value.Id).Order())) throw new InvalidOperationException("Dữ liệu News trên web và database không khớp.");
            await WriteJson($"data/news-{language}.json", new
            {
                articles = listedNews.Select(value => new { value.Id, value.Category, html = newsCards[value.Id] }),
                labels = Text(language, "prev", "next", "Page {0}", "News pages")
            });
        }

        var webRoot = Path.Combine(root, "wwwroot");
        foreach (var path in Directory.EnumerateFiles(webRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(webRoot, path).Replace('\\', '/');
            if (relative.StartsWith("uploads/", StringComparison.Ordinal) || Path.GetFileName(path).StartsWith('.')) continue;
            await Copy(path, relative);
        }
        foreach (var url in uploads.Keys)
        {
            var path = Path.GetFullPath(Path.Combine(webRoot, url.TrimStart('/')));
            if (!path.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !File.Exists(path))
                throw new InvalidOperationException("Không tìm thấy ảnh công khai cần xuất: " + url);
            await Copy(path, url.TrimStart('/'));
        }
        foreach (var asset in Directory.EnumerateFiles(Path.Combine(root, "tools", "Koi.StaticExport", "Assets")))
            await Copy(asset, "static/" + Path.GetFileName(asset));
        await Copy(Path.Combine(root, "tools", "Koi.StaticExport", "Cloudflare.md"), "README.md");
        var coverCss = Path.Combine(staging, "css", "cover.css");
        await File.WriteAllTextAsync(coverCss, (await File.ReadAllTextAsync(coverCss)).Replace(".cover-languages button", ".cover-languages a", StringComparison.Ordinal));
        await Write("404.html", """
            <!DOCTYPE html><html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>404 · KOI Thé</title><link rel="stylesheet" href="/css/cover.css"></head><body><main class="language-cover"><img src="/images/cover-logo.svg" width="210" height="44" alt="KOI Thé"><h1>404</h1><p>Trang không tồn tại / Page not found.</p><p><a href="/">Chọn ngôn ngữ / Choose your language</a></p></main></body></html>
            """);
        await Write("_redirects", "/Home/* /en/Home/:splat 302\n");
        var manifest = new { generator = "Koi.StaticExport", exportedAt = DateTimeOffset.UtcNow, languages = new[] { "vi", "en" }, counts };
        await WriteJson("static-site.json", manifest);
        var files = Directory.GetFiles(staging, "*", SearchOption.AllDirectories);
        if (files.Length > 20000 || files.Any(path => new FileInfo(path).Length > 25 * 1024 * 1024))
            throw new InvalidOperationException("Bản xuất vượt giới hạn file của Cloudflare Pages.");
        if (Directory.Exists(output))
        {
            using var previous = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(output, "static-site.json")));
            if (previous.RootElement.GetProperty("generator").GetString() != "Koi.StaticExport") throw new InvalidOperationException("Không ghi đè thư mục không thuộc công cụ xuất.");
            Directory.Move(output, staging + "-previous");
        }
        Directory.Move(staging, output);
        Console.WriteLine($"Đã xuất {files.Length} file vào fe-demo. Cloudflare Pages: None / exit 0 / fe-demo.");
    }

    private async Task<ConcurrentDictionary<Guid, string>> CaptureCards(string action, string selector, int count, int size, string language, Func<IElement, Guid> getId)
    {
        var cards = new ConcurrentDictionary<Guid, string>();
        await Parallel.ForEachAsync(Enumerable.Range(1, Math.Max(1, (int)Math.Ceiling(count / (double)size))),
            new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (number, _) =>
            {
                var page = await Get($"/Home/{action}?page={number}", language);
                foreach (var card in page.QuerySelectorAll(selector))
                {
                    var id = getId(card);
                    RewriteLinks(card, language);
                    CollectUploads(card);
                    if (!cards.TryAdd(id, card.OuterHtml)) throw new InvalidOperationException($"Mục trùng khi xuất {action}: {id}");
                }
            });
        return cards;
    }

    private async Task<IDocument> Get(string path, string language)
    {
        var url = new Uri(source, path.TrimStart('/') + (path.Contains('?') ? "&" : "?") + "culture=" + language);
        using var response = await client.GetAsync(url);
        if (response.StatusCode != System.Net.HttpStatusCode.OK) throw new InvalidOperationException($"Nguồn trả HTTP {(int)response.StatusCode}: {path}");
        var document = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());
        if (document.QuerySelector(".admin-header") != null) throw new InvalidOperationException("Không xuất trang quản trị.");
        return document;
    }

    private void Prepare(IDocument page, string language, string path, bool isCover = false)
    {
        foreach (var link in page.QuerySelectorAll("a[href^='/Order']")) link.Remove();
        foreach (var input in page.QuerySelectorAll("input[name=__RequestVerificationToken]")) input.Remove();
        foreach (var form in page.QuerySelectorAll("form.language-switcher, .cover-languages form"))
        {
            var navigation = page.CreateElement(isCover ? "span" : "nav");
            navigation.ClassName = form.ClassName;
            if (form.HasAttribute("aria-label")) navigation.SetAttribute("aria-label", form.GetAttribute("aria-label")!);
            foreach (var button in form.QuerySelectorAll("button"))
            {
                var target = button.GetAttribute("value") ?? form.QuerySelector("input[name=culture]")!.GetAttribute("value")!;
                var link = page.CreateElement("a");
                link.ClassName = button.ClassName; link.TextContent = button.TextContent;
                link.SetAttribute("lang", target); link.SetAttribute("data-static-language", target);
                link.SetAttribute("href", "/" + target + (isCover ? "/Home/Index/" : path.TrimEnd('/') + "/"));
                if (!isCover && target == language) link.SetAttribute("aria-current", "true");
                navigation.AppendChild(link);
                if (!isCover && target == "vi") { var separator = page.CreateElement("span"); separator.TextContent = "/"; separator.SetAttribute("aria-hidden", "true"); navigation.AppendChild(separator); }
            }
            form.Replace(navigation);
        }
        RewriteLinks(page.DocumentElement, language);
        CollectUploads(page.DocumentElement);
        var scripts = page.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!).ToArray();
        foreach (var script in page.QuerySelectorAll("script[src]")) script.Remove();
        var settings = page.CreateElement("script"); settings.Id = "static-scripts"; settings.SetAttribute("type", "application/json");
        // Default encoding is used inside HTML so translated text cannot terminate a script element.
        settings.TextContent = JsonSerializer.Serialize(scripts); page.Body!.AppendChild(settings);
        var runtime = page.CreateElement("script"); runtime.SetAttribute("src", "/static/runtime.js"); runtime.SetAttribute("defer", ""); page.Body.AppendChild(runtime);
        var css = page.CreateElement("link"); css.SetAttribute("rel", "stylesheet"); css.SetAttribute("href", "/static/runtime.css"); page.Head!.AppendChild(css);
        page.DocumentElement.SetAttribute("data-static-export", "true");
        if (isCover) return;
        var loading = page.CreateElement("script"); loading.TextContent = "if (/[?&](c|s|b|page|category)=/i.test(location.search)) document.documentElement.classList.add('static-loading');"; page.Head.Prepend(loading);
    }

    private void RewriteLinks(IElement element, string language)
    {
        foreach (var link in element.QuerySelectorAll("a[href]"))
        {
            var href = link.GetAttribute("href")!;
            if (!Uri.TryCreate(source, href, out var url) || url.GetLeftPart(UriPartial.Authority) != source.GetLeftPart(UriPartial.Authority)) continue;
            if (url.AbsolutePath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) || url.AbsolutePath.StartsWith("/Home/NewsPreview", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Phát hiện liên kết quản trị trong nội dung công khai.");
            if (url.AbsolutePath.StartsWith("/Home/", StringComparison.OrdinalIgnoreCase))
                link.SetAttribute("href", "/" + language + url.AbsolutePath.TrimEnd('/') + "/" + url.Query + url.Fragment);
        }
    }

    private void CollectUploads(IElement element)
    {
        foreach (var item in element.QuerySelectorAll("[src], [href], [srcset], [poster], [style]").Prepend(element))
        {
            foreach (var attribute in new[] { "src", "href", "poster" }) Track(item.GetAttribute(attribute) ?? "");
            foreach (var value in (item.GetAttribute("srcset") ?? "").Split(',')) Track(value.Trim().Split(' ')[0]);
            foreach (Match match in Regex.Matches(item.GetAttribute("style") ?? "", "url\\(\\s*['\"]?([^)'\"\\s]+)")) Track(match.Groups[1].Value);
        }

        void Track(string value)
        {
            if (Uri.TryCreate(source, value, out var url) && url.AbsolutePath.StartsWith("/uploads/", StringComparison.Ordinal) && url.GetLeftPart(UriPartial.Authority) == source.GetLeftPart(UriPartial.Authority))
                uploads.TryAdd(Uri.UnescapeDataString(url.AbsolutePath), 0);
        }
    }

    private Dictionary<string, string> Text(string language, params string[] keys) => keys.ToDictionary(key => key, key => labels.GetString(key, CultureInfo.GetCultureInfo(language)) ?? key);
    private static string Html(IDocument page) => "<!DOCTYPE html>\n" + Regex.Replace(page.DocumentElement.OuterHtml, "[ \\t]+(?=\\r?$)", "", RegexOptions.Multiline) + "\n";
    private async Task Write(string relative, string content)
    {
        var path = Path.Combine(staging, relative); Directory.CreateDirectory(Path.GetDirectoryName(path)!); await File.WriteAllTextAsync(path, content);
    }
    private Task WriteJson(string relative, object value) => Write(relative, JsonSerializer.Serialize(value, Json));
    private async Task Copy(string from, string relative)
    {
        var destination = Path.Combine(staging, relative); Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        await using var input = File.OpenRead(from); await using var result = File.Create(destination); await input.CopyToAsync(result);
    }
}
