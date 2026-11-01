using Ganss.Xss;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class NewsService(
    ApplicationDbContext database,
    ImageUploads uploads,
    ILogger<NewsService> logger
)
{
    public async Task<AdminNewsViewModel> GetListAsync(string? search, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 200)
            search = search[..200];
        var query = database.NewsArticles.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(article => (EF.Functions.ILike(article.Title, $"%{search}%")));
        const int pageSize = 20;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var articles = await query
            .OrderByDescending(article => article.Date)
            .ThenByDescending(article => article.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();
        return new AdminNewsViewModel(articles, search, page, pageCount);
    }

    public async Task<NewsEditViewModel?> GetEditAsync(Guid id)
    {
        var article = await database
            .NewsArticles.AsNoTracking()
            .SingleOrDefaultAsync(article => article.Id == id);
        return article == null
            ? null
            : new NewsEditViewModel
            {
                Id = article.Id,
                Revision = article.Revision,
                Title = article.Title,
                Category = article.Category,
                Date = article.Date,
                BodyHtml = article.BodyHtml,
                CurrentImage = article.Image,
                VideoUrl =
                    article.VideoId == null
                        ? null
                        : $"https://www.youtube.com/embed/{article.VideoId}",
                IsPublished = article.IsPublished,
                IsFeatured = article.IsFeatured,
                ShowOnHome = article.ShowOnHome,
                HomeOrder = article.HomeOrder,
            };
    }

    public async Task<SaveResult> SaveAsync(
        NewsEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var article = id.HasValue
            ? await database.NewsArticles.SingleOrDefaultAsync(article => article.Id == id)
            : new NewsArticle();
        if (article == null)
            return SaveResult.NotFound;
        model.CurrentImage = article.Image;
        if (model.IsFeatured && !model.IsPublished)
            modelState.AddModelError(nameof(model.IsFeatured), "Tin nổi bật cần được xuất bản.");
        var videoId = ParseVideoId(model.VideoUrl);
        if (!string.IsNullOrWhiteSpace(model.VideoUrl) && videoId == null)
            modelState.AddModelError(nameof(model.VideoUrl), "Nhập liên kết video YouTube hợp lệ.");
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        var body = sanitizer.Sanitize(model.BodyHtml ?? "").Trim();
        if (string.IsNullOrWhiteSpace(body))
            modelState.AddModelError(nameof(model.BodyHtml), "Nhập nội dung bài viết.");
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        if (id == null && image == null)
            modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh cho bài viết.");
        if (!modelState.IsValid)
        {
            return SaveResult.Invalid;
        }
        string? uploadedFile = null;
        await using var transaction = await database.Database.BeginTransactionAsync();
        try
        {
            if (image != null)
            {
                uploadedFile = await uploads.SaveAsync(image, "news");
                article.Image = article.BannerImage = uploadedFile;
                article.HomeImage = "";
            }
            article.Title = model.Title.Trim();

            article.Category = model.Category;
            article.Date = model.Date!.Value;
            if (
                article.VideoId != null
                && videoId == null
                && string.IsNullOrEmpty(article.BannerImage)
            )
                article.BannerImage = article.Image;
            article.VideoId = videoId;
            article.BodyHtml = body;
            article.IsPublished = model.IsPublished;
            article.IsFeatured = model.IsFeatured;
            article.ShowOnHome = model.ShowOnHome;
            article.HomeOrder = model.HomeOrder;
            if (id == null)
                database.NewsArticles.Add(article);
            else
            {
                database.Entry(article).Property(value => value.Revision).OriginalValue =
                    model.Revision;
                article.Revision++;
            }
            if (article.IsFeatured)
                await database
                    .NewsArticles.Where(other => other.IsFeatured && other.Id != article.Id)
                    .ExecuteUpdateAsync(update =>
                        update.SetProperty(other => other.IsFeatured, false)
                    );
            await database.SaveChangesAsync();
            await transaction.CommitAsync();
            return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            modelState.AddModelError(
                "",
                "Bài viết đã được thay đổi. Hãy tải lại trang sửa trước khi lưu tiếp."
            );
            uploads.Rollback(uploadedFile);
            return SaveResult.Conflict;
        }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save news article {ArticleId}", id);
            modelState.AddModelError("", "Chưa lưu được bài viết. Vui lòng thử lại.");
            uploads.Rollback(uploadedFile);
            return SaveResult.Unavailable;
        }
    }

    public Task<NewsArticle?> GetArticleAsync(Guid id) =>
        database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);

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
