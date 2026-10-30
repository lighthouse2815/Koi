using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Koi.Web.Models;
using Koi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Koi.Web.Controllers;

public class HomeController(ApplicationDbContext database) : Controller
{
    public IActionResult Cover()
    {
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var news = await database.NewsArticles.AsNoTracking()
            .Where(article => article.IsPublished && article.ShowOnHome)
            .OrderBy(article => article.HomeOrder).ThenByDescending(article => article.Date)
            .Take(6).ToArrayAsync();
        return View(new HomeViewModel(news));
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult OurTea()
    {
        return View();
    }

    public IActionResult Recruit()
    {
        return View();
    }

    public async Task<IActionResult> News(string? category, int page = 1)
    {
        category = category?.ToUpperInvariant();
        if (category is not ("NEWS" or "MEDIA")) category = null;
        var published = database.NewsArticles.AsNoTracking().Where(article => article.IsPublished);
        var featured = await published.OrderByDescending(article => article.IsFeatured)
            .ThenByDescending(article => article.Date).ThenByDescending(article => article.Id).FirstOrDefaultAsync();
        var featuredId = featured?.Id ?? 0;
        var articles = published.Where(article => article.Id != featuredId && (category == null || article.Category == category));
        const int pageSize = 5;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await articles.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        return View(new NewsViewModel(
            featured,
            await articles.OrderByDescending(article => article.Date).ThenByDescending(article => article.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(),
            category, page, pageCount));
    }

    public async Task<IActionResult> NewsDetail(int id)
    {
        var article = await database.NewsArticles.AsNoTracking().FirstOrDefaultAsync(article => article.Id == id && article.IsPublished);
        return article == null ? NotFound() : View(article);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewsPreview(int id)
    {
        var article = await database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);
        if (article == null) return NotFound();
        ViewData["IsAdminPreview"] = true;
        return View("NewsDetail", article);
    }

    public IActionResult Store(int c = 0, int s = 0, int b = 0, int page = 1)
    {
        var catalog = StoreCatalog.Snapshot;
        var country = catalog.Countries.FirstOrDefault(country => country.Id == c);
        c = country?.Id ?? 0;
        var locations = catalog.Locations.Where(location => location.CountryId == c).ToArray();
        var location = locations.FirstOrDefault(location => location.Id == s);
        s = location?.Id ?? 0;
        var brands = catalog.Brands.Where(brand => country == null || country.BrandIds.Contains(brand.Id)).ToArray();
        if (!brands.Any(brand => brand.Id == b)) b = 0;
        // The reference orders each country/city independently of the global list.
        var orderedIds = location?.StoreIds ?? country?.StoreIds;
        var orderedStores = orderedIds == null ? catalog.Stores : orderedIds.Select(id => catalog.Stores[id - 1]);
        var stores = orderedStores.Where(store => (c == 0 || store.CountryId == c)
            && (s == 0 || store.LocationId == s) && (b == 0 || store.BrandId == b)).ToArray();
        const int pageSize = 9;
        var pageCount = Math.Max(1, (int)Math.Ceiling(stores.Length / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        return View(new StoreViewModel(catalog.Countries, locations, brands,
            stores.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
            country?.SocialLinks ?? [], c, s, b, page, pageCount));
    }

    public IActionResult Impact()
    {
        return View();
    }

    public IActionResult Responsibility() => View();
    public IActionResult Precision() => View();
    public IActionResult People() => View();
    public IActionResult Connections() => View();
    public IActionResult Contact() => View();
    public IActionResult Menu() => View(new MenuViewModel(MenuCatalog.Countries));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
