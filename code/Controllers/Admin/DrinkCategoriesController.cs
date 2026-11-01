using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinkCategoriesController(DrinkCategoryService categories) : Controller
{
    public async Task<IActionResult> Index() => View(await categories.GetListAsync());
    public IActionResult Create() => View("Edit", new DrinkCategoryEditViewModel());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await categories.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Create(DrinkCategoryEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, DrinkCategoryEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(DrinkCategoryEditViewModel model, Guid? id)
    {
        var result = await categories.SaveAsync(model, id, ModelState);
        if (result == SaveResult.NotFound) return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu danh mục đồ uống.";
            return RedirectToAction(nameof(Index));
        }
        Response.StatusCode = result switch
        {
            SaveResult.Invalid => StatusCodes.Status400BadRequest,
            SaveResult.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status503ServiceUnavailable
        };
        return View("Edit", model);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var model = await categories.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await categories.DeleteAsync(id, ModelState);
        if (result == SaveResult.NotFound) return NotFound();
        if (result == SaveResult.Conflict)
        {
            var model = await categories.GetDeleteAsync(id);
            if (model == null) return NotFound();
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View("~/code/Views/Admin/Shared/Delete.cshtml", model);
        }
        TempData["Status"] = "Đã xóa danh mục đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
