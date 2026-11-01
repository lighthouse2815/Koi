using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class SiteContentService(ApplicationDbContext database)
{
    public async Task<HomeViewModel> GetHomeAsync()
    {
        var news = await database.NewsArticles.AsNoTracking()
            .Where(article => article.IsPublished && article.ShowOnHome)
            .OrderBy(article => article.HomeOrder).ThenByDescending(article => article.Date)
            .Take(6).ToArrayAsync();
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished && value.ShowOnHome)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return new HomeViewModel(news, categories);
    }

    public async Task<OurTeaViewModel> GetOurTeaAsync()
    {
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished)
            .Include(value => value.Drinks.Where(drink => drink.IsPublished).OrderBy(drink => drink.SortOrder).ThenBy(drink => drink.Id))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).AsSplitQuery().ToArrayAsync();
        var featured = await database.FeaturedDrinks.AsNoTracking().Where(value => value.IsPublished)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return new OurTeaViewModel(categories, featured);
    }

    public async Task<NewsViewModel> GetNewsAsync(string? category, int page = 1)
    {
        category = category?.ToUpperInvariant();
        if (category is not ("NEWS" or "MEDIA")) category = null;
        var categoryFilter = category == null ? (NewsCategory?)null : Enum.Parse<NewsCategory>(category);
        var published = database.NewsArticles.AsNoTracking().Where(article => article.IsPublished);
        var featured = await published.OrderByDescending(article => article.IsFeatured)
            .ThenByDescending(article => article.Date).ThenByDescending(article => article.Id).FirstOrDefaultAsync();
        var featuredId = featured?.Id ?? Guid.Empty;
        var articles = published.Where(article => article.Id != featuredId && (categoryFilter == null || article.Category == categoryFilter));
        const int pageSize = 5;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await articles.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        return new NewsViewModel(
            featured,
            await articles.OrderByDescending(article => article.Date).ThenByDescending(article => article.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(),
            category, page, pageCount);
    }

    public async Task<StoreViewModel> GetStoresAsync(Guid c = default, Guid s = default, Guid b = default, int page = 1)
    {
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        var cities = await database.StoreCities.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        if (!cities.Any(value => value.Id == s)) s = Guid.Empty;
        var query = database.Stores.AsNoTracking().Where(value => value.IsPublished && (s == Guid.Empty || value.CityId == s));
        const int pageSize = 9;
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        var rows = await query.OrderBy(value => s != Guid.Empty ? value.CityOrder : value.CountryOrder).ThenBy(value => value.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        var stores = rows.Select(value => new StoreBranch(value.Id, value.Name, value.Phone,
            value.Hours, value.Address, shop.Logo, value.Photo, value.NavigationUrl)).ToArray();
        var links = await database.SocialLinks.AsNoTracking().OrderBy(value => value.SortOrder).Select(value => new StoreSocialLink(value.Url, value.Label)).ToArrayAsync();
        return new StoreViewModel(cities.Select(value => new StoreLocation(value.Id, value.Name)).ToArray(), stores, links, shop.Name, s, page, pages);
    }

    public async Task<MenuViewModel> GetMenusAsync()
    {
        var menus = await database.Menus.AsNoTracking().Where(value => value.IsPublished).OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return new MenuViewModel(menus.Select(value => new MenuDocument(value.Label, value.File)).ToArray());
    }

    public Task<NewsArticle?> GetArticleAsync(Guid id, bool publishedOnly) =>
        database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id && (!publishedOnly || article.IsPublished));
}
