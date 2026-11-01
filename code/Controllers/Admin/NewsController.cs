using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class NewsController(NewsService news) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1) => View(await news.GetListAsync(search, page));

    [HttpGet]
    public IActionResult Create() => View("Edit", new NewsEditViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await news.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public IActionResult Preview(Guid id) => RedirectToAction("NewsPreview", "Home", new { area = "", id });

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(NewsEditViewModel model) => Save(model, null);

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, NewsEditViewModel model) => id != model.Id
        ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(NewsEditViewModel model, Guid? id)
    {
        var status = await news.SaveAsync(model, id, ModelState);
        if (status == SaveResult.NotFound) return NotFound();
        if (status == SaveResult.Success)
        {
            TempData["Status"] = id.HasValue ? "Đã cập nhật bài viết." : "Đã thêm bài viết.";
            return RedirectToAction(nameof(Index));
        }
        Response.StatusCode = status switch
        {
            SaveResult.Invalid => StatusCodes.Status400BadRequest,
            SaveResult.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status503ServiceUnavailable
        };
        return View("Edit", model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var article = await news.GetArticleAsync(id);
        return article == null ? NotFound() : View(article);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await news.DeleteAsync(id)) return NotFound();
        TempData["Status"] = "Đã xóa bài viết.";
        return RedirectToAction(nameof(Index));
    }
}
