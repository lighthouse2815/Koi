using Koi.Web.Models;
using Koi.Web.Filters;
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
    public IActionResult Preview(Guid id) => RedirectToAction("NewsPreview", "Home", new { area = "", id });

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create(NewsEditViewModel model)
    {
        await news.CreateAsync(model);
        TempData["Status"] = "Đã thêm bài viết.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, NewsEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await news.UpdateAsync(id, model);
        TempData["Status"] = "Đã cập nhật bài viết.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await news.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa bài viết.";
        return RedirectToAction(nameof(Index));
    }
}
