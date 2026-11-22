using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class SiteContentService(ApplicationDbContext database, IConfiguration configuration)
{
    public async Task<NewsViewModel> GetNewsAsync(string? category, int page = 1)
    {
        category = category?.ToUpperInvariant();
        if (category is not ("NEWS" or "MEDIA"))
            category = null;
        var categoryFilter =
            category == null ? (NewsCategory?)null : Enum.Parse<NewsCategory>(category);
        var allArticles = database.NewsArticles.AsNoTracking();
        var featured = await allArticles
            .OrderByDescending(article => article.IsFeatured)
            .ThenByDescending(article => article.Date)
            .ThenByDescending(article => article.Id)
            .FirstOrDefaultAsync();
        var featuredId = featured?.Id ?? Guid.Empty;
        var articles = allArticles.Where(article =>
            article.Id != featuredId
            && (categoryFilter == null || article.Category == categoryFilter)
        );
        const int pageSize = 5;
        var pageCount = Math.Max(
            1,
            (int)Math.Ceiling(await articles.CountAsync() / (double)pageSize)
        );
        page = Math.Clamp(page, 1, pageCount);
        return new NewsViewModel(
            featured,
            await articles
                .OrderByDescending(article => article.Date)
                .ThenByDescending(article => article.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(),
            category,
            page,
            pageCount
        );
    }

    public async Task<StoreViewModel> GetStoresAsync(Guid s = default, int page = 1)
    {
        var brandName =
            configuration["Brand:Name"]
            ?? throw new InvalidOperationException("Configure Brand:Name.");
        var brandLogo =
            configuration["Brand:Logo"]
            ?? throw new InvalidOperationException("Configure Brand:Logo.");
        var cities = await database
            .StoreCities.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArrayAsync();
        if (!cities.Any(value => value.Id == s))
            s = Guid.Empty;
        var query = database
            .Stores.AsNoTracking()
            .Where(value => s == Guid.Empty || value.CityId == s);
        const int pageSize = 9;
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        var rows = await query
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
        var stores = rows.Select(value => new StoreBranch(
                value.Id,
                value.Name,
                value.Phone,
                value.Hours,
                value.Address,
                brandLogo,
                value.Photo,
                value.NavigationUrl
            ))
            .ToArray();
        return new StoreViewModel(
            cities.Select(value => new StoreLocation(value.Id, value.Name)).ToArray(),
            stores,
            brandName,
            s,
            page,
            pages
        );
    }

    public Task<NewsArticle?> GetArticleAsync(Guid id) =>
        database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);
}
