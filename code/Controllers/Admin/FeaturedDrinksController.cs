using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class FeaturedDrinksController(FeaturedDrinkAdminService drinks) : Controller
{
    public async Task<IActionResult> Index() => View(await drinks.GetListAsync());
    public IActionResult Create() => View("Edit", new FeaturedDrinkEditViewModel());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await drinks.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    public Task<IActionResult> Create(FeaturedDrinkEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, FeaturedDrinkEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(FeaturedDrinkEditViewModel model, Guid? id)
    {
        var result = await drinks.SaveAsync(model, id, ModelState);
        if (result == SaveResult.NotFound) return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu mục New Drinks.";
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
        var model = await drinks.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await drinks.DeleteAsync(id)) return NotFound();
        TempData["Status"] = "Đã xóa mục New Drinks.";
        return RedirectToAction(nameof(Index));
    }
}
