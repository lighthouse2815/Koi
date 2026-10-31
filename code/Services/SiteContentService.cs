using Koi.Web.Data;
using Koi.Web.Localization;
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
            .OrderBy(value => value.HomeOrder).ThenBy(value => value.Id).ToArrayAsync();
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
        var published = database.NewsArticles.AsNoTracking().Where(article => article.IsPublished);
        var featured = await published.OrderByDescending(article => article.IsFeatured)
            .ThenByDescending(article => article.Date).ThenByDescending(article => article.Id).FirstOrDefaultAsync();
        var featuredId = featured?.Id ?? Guid.Empty;
        var articles = published.Where(article => article.Id != featuredId && (category == null || article.Category == category));
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
        var shopBrandId = shop.BrandId;
        if (shop.MarketId.HasValue) c = shop.MarketId.Value;
        var markets = await database.Markets.AsNoTracking().Include(value => value.Brands).Include(value => value.SocialLinks)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).AsSplitQuery().ToArrayAsync();
        var country = markets.FirstOrDefault(value => value.Id == c);
        c = country?.Id ?? Guid.Empty;
        var cities = await database.StoreCities.AsNoTracking().Where(value => value.MarketId == c)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var city = cities.FirstOrDefault(value => value.Id == s);
        s = city?.Id ?? Guid.Empty;
        var brandIds = country?.Brands.Select(value => value.BrandId).ToArray();
        var brands = await database.TeaBrands.AsNoTracking().Where(value => (brandIds == null || brandIds.Contains(value.Id)) && (!shopBrandId.HasValue || value.Id == shopBrandId))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        if (!brands.Any(value => value.Id == b)) b = Guid.Empty;
        var query = database.Stores.AsNoTracking().Where(value => value.IsPublished && (!shopBrandId.HasValue || value.BrandId == shopBrandId) && (c == Guid.Empty || value.MarketId == c)
            && (s == Guid.Empty || value.CityId == s) && (b == Guid.Empty || value.BrandId == b));
        const int pageSize = 9;
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        var stores = await query.OrderBy(value => s != Guid.Empty ? value.CityOrder : c != Guid.Empty ? value.CountryOrder : value.SortOrder)
            .ThenBy(value => value.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(value => new StoreBranch(value.Id, SiteText.Select(value.Name, value.NameVi), value.Phone, SiteText.Select(value.Hours, value.HoursVi), SiteText.Select(value.Address, value.AddressVi),
                value.BrandId, value.Brand.Logo, value.Photo, value.NavigationUrl)).ToArrayAsync();
        return new StoreViewModel(markets.Select(value => new StoreCountry(value.Id, SiteText.Select(value.Name, value.NameVi))).ToArray(),
            cities.Select(value => new StoreLocation(value.Id, SiteText.Select(value.Name, value.NameVi))).ToArray(),
            brands.Select(value => new StoreBrand(value.Id, SiteText.Select(value.Name, value.NameVi))).ToArray(), stores,
            country?.SocialLinks.OrderBy(value => value.SortOrder).Select(value => new StoreSocialLink(value.Url, value.Label)).ToArray() ?? [],
            c, s, b, page, pages);
    }

    public async Task<MenuViewModel> GetMenusAsync()
    {
        var marketId = await database.ShopSettings.AsNoTracking().Where(value => value.Id == 1).Select(value => value.MarketId).SingleAsync();
        var menus = await database.Menus.AsNoTracking().Where(value => value.IsPublished && (!marketId.HasValue || value.MarketId == marketId)).Include(value => value.Market)
            .OrderBy(value => value.Market.SortOrder).ThenBy(value => value.MarketId).ThenBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        return new MenuViewModel(menus.GroupBy(value => value.MarketId).Select(group =>
            new MenuCountry(SiteText.Select(group.First().Market.Name, group.First().Market.NameVi), group.Select(value => new MenuDocument(SiteText.Select(value.Label, value.LabelVi), value.File)).ToArray())).ToArray());
    }

    public Task<NewsArticle?> GetArticleAsync(Guid id, bool publishedOnly) =>
        database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id && (!publishedOnly || article.IsPublished));
}
