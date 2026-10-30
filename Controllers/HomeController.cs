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
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished && value.ShowOnHome)
            .OrderBy(value => value.HomeOrder).ThenBy(value => value.Id).ToArrayAsync();
        return View(new HomeViewModel(news, categories));
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> OurTea()
    {
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished)
            .Include(value => value.Drinks.Where(drink => drink.IsPublished).OrderBy(drink => drink.SortOrder).ThenBy(drink => drink.Id))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).AsSplitQuery().ToArrayAsync();
        var featured = await database.FeaturedDrinks.AsNoTracking().Where(value => value.IsPublished)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return View(new OurTeaViewModel(categories, featured));
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

    public async Task<IActionResult> Store(int c = 0, int s = 0, int b = 0, int page = 1)
    {
        var markets = await database.Markets.AsNoTracking().Include(value => value.Brands).Include(value => value.SocialLinks)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).AsSplitQuery().ToArrayAsync();
        var country = markets.FirstOrDefault(value => value.Id == c);
        c = country?.Id ?? 0;
        var cities = await database.StoreCities.AsNoTracking().Where(value => value.MarketId == c)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var city = cities.FirstOrDefault(value => value.Id == s);
        s = city?.Id ?? 0;
        var brandIds = country?.Brands.Select(value => value.BrandId).ToArray();
        var brands = await database.TeaBrands.AsNoTracking().Where(value => brandIds == null || brandIds.Contains(value.Id))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        if (!brands.Any(value => value.Id == b)) b = 0;
        var query = database.Stores.AsNoTracking().Where(value => value.IsPublished && (c == 0 || value.MarketId == c)
            && (s == 0 || value.CityId == s) && (b == 0 || value.BrandId == b));
        const int pageSize = 9;
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        var stores = await query.OrderBy(value => s != 0 ? value.CityOrder : c != 0 ? value.CountryOrder : value.SortOrder)
            .ThenBy(value => value.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(value => new StoreBranch(value.Id, value.Name, value.Phone, value.Hours, value.Address,
                value.MarketId, value.CityId ?? 0, value.BrandId, value.Brand.Logo, value.Photo, value.NavigationUrl)).ToArrayAsync();
        return View(new StoreViewModel(markets.Select(value => new StoreCountry(value.Id, value.Name,
                value.Brands.Select(value => value.BrandId).ToArray(), [], [])).ToArray(),
            cities.Select(value => new StoreLocation(value.Id, value.MarketId, value.Name, [])).ToArray(),
            brands.Select(value => new StoreBrand(value.Id, value.Name)).ToArray(), stores,
            country?.SocialLinks.OrderBy(value => value.SortOrder).Select(value => new StoreSocialLink(value.Url, value.Label)).ToArray() ?? [],
            c, s, b, page, pages));
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
    public async Task<IActionResult> Menu()
    {
        var menus = await database.Menus.AsNoTracking().Where(value => value.IsPublished).Include(value => value.Market)
            .OrderBy(value => value.Market.SortOrder).ThenBy(value => value.MarketId).ThenBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return View(new MenuViewModel(menus.GroupBy(value => value.MarketId).Select(group =>
            new MenuCountry(group.First().Market.Name, group.Select(value => new MenuDocument(value.Label, value.File)).ToArray())).ToArray()));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
