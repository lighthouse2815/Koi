using Ganss.Xss;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class NewsController(ApplicationDbContext database, IWebHostEnvironment environment, ILogger<NewsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 200) search = search[..200];
        var query = database.NewsArticles.AsNoTracking();
        if (!string.IsNullOrEmpty(search)) query = query.Where(article => EF.Functions.ILike(article.Title, $"%{search}%"));
        const int pageSize = 20;
        var pageCount = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var articles = await query.OrderByDescending(article => article.Date).ThenByDescending(article => article.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        return View(new AdminNewsViewModel(articles, search, page, pageCount));
    }

    [HttpGet]
    public IActionResult Create() => View("Edit", new NewsEditViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);
        return article == null ? NotFound() : View(new NewsEditViewModel
        {
            Id = article.Id, Revision = article.Revision, Title = article.Title, Category = article.Category,
            Date = article.Date, BodyHtml = article.BodyHtml, CurrentImage = article.Image,
            VideoUrl = article.VideoId == null ? null : $"https://www.youtube.com/embed/{article.VideoId}",
            IsPublished = article.IsPublished, IsFeatured = article.IsFeatured,
            ShowOnHome = article.ShowOnHome, HomeOrder = article.HomeOrder
        });
    }

    [HttpGet]
    public IActionResult Preview(int id) => RedirectToAction("NewsPreview", "Home", new { area = "", id });

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(NewsEditViewModel model) => Save(model, null);

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, NewsEditViewModel model) => id != model.Id
        ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(NewsEditViewModel model, int? id)
    {
        var article = id.HasValue ? await database.NewsArticles.SingleOrDefaultAsync(article => article.Id == id) : new NewsArticle();
        if (article == null) return NotFound();
        model.CurrentImage = article.Image;
        if (model.IsFeatured && !model.IsPublished) ModelState.AddModelError(nameof(model.IsFeatured), "Tin nổi bật cần được xuất bản.");
        var videoId = ParseVideoId(model.VideoUrl);
        if (!string.IsNullOrWhiteSpace(model.VideoUrl) && videoId == null)
            ModelState.AddModelError(nameof(model.VideoUrl), "Nhập liên kết video YouTube hợp lệ.");
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        var body = sanitizer.Sanitize(model.BodyHtml ?? "").Trim();
        if (string.IsNullOrWhiteSpace(body)) ModelState.AddModelError(nameof(model.BodyHtml), "Nhập nội dung bài viết.");
        byte[]? imageBytes = null;
        string? extension = null;
        if (model.ImageUpload is { Length: > 0 } image)
        {
            extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (image.Length > 5 * 1024 * 1024 || extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                ModelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.");
            else
            {
                using var buffer = new MemoryStream();
                await image.CopyToAsync(buffer);
                imageBytes = buffer.ToArray();
                if (!IsImage(imageBytes, extension)) ModelState.AddModelError(nameof(model.ImageUpload), "Nội dung tệp không đúng định dạng ảnh.");
            }
        }
        else if (id == null) ModelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh cho bài viết.");
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View("Edit", model);
        }
        string? uploadedFile = null;
        await using var transaction = await database.Database.BeginTransactionAsync();
        try
        {
            if (imageBytes != null)
            {
                var name = Guid.NewGuid().ToString("N") + extension;
                var directory = Path.Combine(environment.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(directory);
                uploadedFile = Path.Combine(directory, name);
                await System.IO.File.WriteAllBytesAsync(uploadedFile, imageBytes);
                article.Image = article.BannerImage = "/uploads/news/" + name;
                article.HomeImage = "";
            }
            article.Title = model.Title.Trim();
            article.Category = model.Category;
            article.Date = model.Date!.Value;
            if (article.VideoId != null && videoId == null && string.IsNullOrEmpty(article.BannerImage))
                article.BannerImage = article.Image;
            article.VideoId = videoId;
            article.BodyHtml = body;
            article.IsPublished = model.IsPublished;
            article.IsFeatured = model.IsFeatured;
            article.ShowOnHome = model.ShowOnHome;
            article.HomeOrder = model.HomeOrder;
            if (id == null) database.NewsArticles.Add(article);
            else
            {
                database.Entry(article).Property(value => value.Revision).OriginalValue = model.Revision;
                article.Revision++;
            }
            if (article.IsFeatured)
                await database.NewsArticles.Where(other => other.IsFeatured && other.Id != article.Id)
                    .ExecuteUpdateAsync(update => update.SetProperty(other => other.IsFeatured, false));
            await database.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Status"] = id.HasValue ? "Đã cập nhật bài viết." : "Đã thêm bài viết.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "Bài viết đã được thay đổi. Hãy tải lại trang sửa trước khi lưu tiếp.");
            Response.StatusCode = StatusCodes.Status409Conflict;
        }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save news article {ArticleId}", id);
            ModelState.AddModelError("", "Chưa lưu được bài viết. Vui lòng thử lại.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
        if (uploadedFile != null) System.IO.File.Delete(uploadedFile);
        return View("Edit", model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await database.NewsArticles.AsNoTracking().SingleOrDefaultAsync(article => article.Id == id);
        return article == null ? NotFound() : View(article);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var article = await database.NewsArticles.FindAsync(id);
        if (article == null) return NotFound();
        database.NewsArticles.Remove(article);
        await database.SaveChangesAsync();
        TempData["Status"] = "Đã xóa bài viết.";
        return RedirectToAction(nameof(Index));
    }

    private static bool IsImage(byte[] data, string extension) => extension switch
    {
        ".png" => data.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        ".jpg" or ".jpeg" => data.AsSpan().StartsWith(new byte[] { 255, 216, 255 }),
        ".webp" => data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) && data.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };

    private static string? ParseVideoId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "https") return null;
        string? id = uri.Host switch
        {
            "youtu.be" => uri.AbsolutePath.Trim('/'),
            "youtube.com" or "www.youtube.com" when uri.AbsolutePath.StartsWith("/embed/") => uri.AbsolutePath[7..],
            "youtube.com" or "www.youtube.com" when uri.AbsolutePath == "/watch" => Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).GetValueOrDefault("v").ToString(),
            _ => null
        };
        return id is { Length: 11 } && id.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-') ? id : null;
    }
}
