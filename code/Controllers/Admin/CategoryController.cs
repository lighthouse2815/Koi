using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoryController(CategoryService categories) : Controller
{
    public async Task<IActionResult> Index() => View(await categories.GetListAsync());
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public async Task<IActionResult> Create(CategoryEditViewModel model)
    {
        await categories.CreateAsync(model, ModelState);
        TempData["Status"] = "Đã lưu danh mục đồ uống.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public async Task<IActionResult> Edit([FromForm] CategoryEditViewModel model)
    {
        await categories.UpdateAsync(model, ModelState);
        TempData["Status"] = "Đã lưu danh mục đồ uống.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await categories.DeleteAsync(id);
        TempData["Status"] = "Đã xóa danh mục đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
