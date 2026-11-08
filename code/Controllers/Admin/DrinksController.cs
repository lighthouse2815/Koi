using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinksController(DrinkService drinks) : Controller
{
    public async Task<IActionResult> Index(string? search, Guid categoryId = default, int page = 1) => View(await drinks.GetListAsync(search, categoryId, page));
    public async Task<IActionResult> Create() => View("Edit", await drinks.CreateAsync());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await drinks.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Create(DrinkEditViewModel model)
    {
        await drinks.CreateAsync(model);
        TempData["Status"] = "Đã lưu đồ uống.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { categoryId = model.CategoryId }) });
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, DrinkEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await drinks.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu đồ uống.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { categoryId = model.CategoryId }) });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var model = await drinks.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await drinks.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
