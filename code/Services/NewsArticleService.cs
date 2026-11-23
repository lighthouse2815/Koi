using Ganss.Xss;
using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class NewsArticleService(ApplicationDbContext database)
{
    public Task<List<NewsArticle>> GetForHomeAsync() =>
        database.NewsArticles
            .AsNoTracking()
            .Where(article => article.ShowOnHome)
            .OrderBy(article => article.HomeOrder)
            .ThenByDescending(article => article.Date)
            .Take(6)
            .ToListAsync();

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
                .ToListAsync(),
            category,
            page,
            pageCount
        );
    }

    public Task<NewsArticle?> GetArticleAsync(Guid id) =>
        database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);

    public async Task<AdminNewsViewModel> GetListAsync(string? search, int page = 1)
    {
        search = search?.Trim();

        var query = database.NewsArticles.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(article => EF.Functions.ILike(article.Title, $"%{search}%"));

        const int pageSize = 20;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);

        var articles = await query
            .OrderByDescending(article => article.Date)
            .ThenByDescending(article => article.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new AdminNewsViewModel(articles, search, page, pageCount);
    }

    public async Task CreateAsync(NewsEditViewModel model)
    {
        var article = new NewsArticle();
        ApplyChanges(article, model, isNew: true);
        database.NewsArticles.Add(article);
        await SaveChangesAsync(article);
    }

    public async Task UpdateAsync(Guid id, NewsEditViewModel model)
    {
        var article = await database.NewsArticles.FindAsync(id)
            ?? throw new AppException(ErrorCode.NEWS_NOT_FOUND);

        ApplyChanges(article, model, isNew: false);
        database.Entry(article).Property(value => value.Revision).OriginalValue = model.Revision;
        article.Revision++;
        await SaveChangesAsync(article);
    }

    private static void ApplyChanges(NewsArticle article, NewsEditViewModel model, bool isNew)
    {
        var videoId = ParseVideoId(model.VideoUrl);
        if (!string.IsNullOrWhiteSpace(model.VideoUrl) && videoId == null)
            throw new AppException(ErrorCode.NEWS_VIDEO_INVALID);
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        var body = sanitizer.Sanitize(model.BodyHtml ?? "").Trim();
        if (string.IsNullOrWhiteSpace(body))
            throw new AppException(ErrorCode.NEWS_BODY_REQUIRED);
        var image = model.ImageUploadUrl;
        if (isNew && string.IsNullOrWhiteSpace(image))
            throw new AppException(ErrorCode.NEWS_IMAGE_REQUIRED);

        if (!string.IsNullOrWhiteSpace(image))
        {
            article.Image = article.BannerImage = image;
            article.HomeImage = "";
        }
        article.Title = model.Title.Trim();

        article.Category = model.Category;
        article.Date = model.Date!.Value;
        if (article.VideoId != null && videoId == null && string.IsNullOrEmpty(article.BannerImage))
            article.BannerImage = article.Image;
        article.VideoId = videoId;
        article.BodyHtml = body;
        article.IsFeatured = model.IsFeatured;
        article.ShowOnHome = model.ShowOnHome;
        article.HomeOrder = model.HomeOrder;
    }

    private async Task SaveChangesAsync(NewsArticle article)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        if (article.IsFeatured)
            await database.NewsArticles
                .Where(other => other.IsFeatured && other.Id != article.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(other => other.IsFeatured, false));
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await database.NewsArticles.FindAsync(id);
        if (article == null)
            return false;
        database.NewsArticles.Remove(article);
        await database.SaveChangesAsync();
        return true;
    }

    private static string? ParseVideoId(string? value)
    {
        if (
            string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != "https"
        )
            return null;
        string? id = uri.Host switch
        {
            "youtu.be" => uri.AbsolutePath.Trim('/'),
            "youtube.com" or "www.youtube.com" when uri.AbsolutePath.StartsWith("/embed/") =>
                uri.AbsolutePath[7..],
            "youtube.com" or "www.youtube.com" when uri.AbsolutePath == "/watch" => Microsoft
                .AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query)
                .GetValueOrDefault("v")
                .ToString(),
            _ => null,
        };
        return
            id is { Length: 11 }
            && id.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-')
            ? id
            : null;
    }
}
